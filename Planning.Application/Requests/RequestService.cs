using Planning.Application.Common;
using Planning.Domain.Availability;
using Planning.Domain.Enums;
using Planning.Domain.Users;

namespace Planning.Application.Requests;

/// <summary>
/// The request inbox. Reads across the entities a request can live in and turns one decision into
/// the right call on the right entity; the entities themselves keep their own invariants.
/// </summary>
public class RequestService : TenantServiceBase, IRequestService
{
    private readonly IAvailabilityRuleRepository _ruleRepository;
    private readonly IUserRepository _userRepository;

    public RequestService(
        IAvailabilityRuleRepository ruleRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
        : base(currentUserContext)
    {
        _ruleRepository = ruleRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<RequestListResponse>> ListAsync(
        bool includeDecided,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<RequestListResponse>();
        }

        // A planner sees the whole organization, anyone else only their own requests. Rules carry a
        // free-text reason, so this is a privacy boundary, not a convenience filter.
        IReadOnlyList<Guid>? employeeIds = CanDecide()
            ? null
            : CurrentUser.UserId is { } ownId ? new List<Guid> { ownId } : [];

        var rules = await _ruleRepository.GetForOrganizationAsync(
            organizationId,
            employeeIds,
            includeDecided,
            cancellationToken);

        var employeesById = (await _userRepository.GetByOrganizationIdAsync(organizationId, cancellationToken))
            .ToDictionary(x => x.Id);

        var items = rules
            .Select(rule => RequestMapper.ToResponse(rule, employeesById.GetValueOrDefault(rule.EmployeeId)))
            // Anything still waiting on someone comes first; within that, newest on top.
            .OrderBy(item => item.Status == ApprovalStatus.Pending ? 0 : 1)
            .ThenByDescending(item => item.CreatedAtUtc)
            .ToList();

        return Result<RequestListResponse>.Success(new RequestListResponse(items));
    }

    public Task<Result<DecideRequestsResponse>> ApproveAsync(
        DecideRequestsRequest request,
        CancellationToken cancellationToken = default) =>
        DecideAsync(request, approve: true, cancellationToken);

    public Task<Result<DecideRequestsResponse>> RejectAsync(
        DecideRequestsRequest request,
        CancellationToken cancellationToken = default) =>
        DecideAsync(request, approve: false, cancellationToken);

    private async Task<Result<DecideRequestsResponse>> DecideAsync(
        DecideRequestsRequest request,
        bool approve,
        CancellationToken cancellationToken)
    {
        if (!TryGetOrganizationId(out _) || CurrentUser.UserId is not { } deciderId)
        {
            return Failures.NoOrganizationContext<DecideRequestsResponse>();
        }

        if (!CanDecide())
        {
            return Failures.ForbiddenFor<DecideRequestsResponse>("You are not allowed to decide on requests.");
        }

        var utcNow = DateTime.UtcNow;
        var decided = 0;
        var skipped = 0;

        foreach (var reference in request.Requests)
        {
            // Every kind lives in the availability table today. The ref carries the kind anyway, so
            // a shift swap can be its own table later without changing this route.
            var rule = await _ruleRepository.GetByIdAsync(reference.Id, cancellationToken);

            // An unknown id, another tenant's id and an already decided request are all "skipped":
            // telling them apart would confirm that the id exists somewhere.
            if (rule is null || !Owns(rule) || rule.ApprovalStatus != ApprovalStatus.Pending)
            {
                skipped++;
                continue;
            }

            if (approve)
            {
                rule.Approve(deciderId, utcNow);
            }
            else
            {
                rule.Reject(deciderId, utcNow);
            }

            await _ruleRepository.UpdateAsync(rule, cancellationToken);
            decided++;
        }

        return Result<DecideRequestsResponse>.Success(new DecideRequestsResponse(decided, skipped));
    }

    private bool CanDecide() =>
        CurrentUser.Role is UserRole.Owner or UserRole.Admin or UserRole.Planner;
}
