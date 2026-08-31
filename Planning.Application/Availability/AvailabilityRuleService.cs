using Planning.Application.Common;
using Planning.Domain.Availability;
using Planning.Domain.Enums;
using Planning.Domain.Planning;
using Planning.Domain.Users;

namespace Planning.Application.Availability;

public class AvailabilityRuleService : TenantServiceBase, IAvailabilityRuleService
{
    private readonly IAvailabilityRuleRepository _ruleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPlanningRecordRepository _planningRecordRepository;

    public AvailabilityRuleService(
        IAvailabilityRuleRepository ruleRepository,
        IUserRepository userRepository,
        IPlanningRecordRepository planningRecordRepository,
        ICurrentUserContext currentUserContext)
        : base(currentUserContext)
    {
        _ruleRepository = ruleRepository;
        _userRepository = userRepository;
        _planningRecordRepository = planningRecordRepository;
    }

    public async Task<Result<AvailabilityRulesListResponse>> ListByEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<AvailabilityRulesListResponse>();
        }

        if (!await CanManageForEmployeeAsync(employeeId, organizationId, cancellationToken))
        {
            return Failures.ForbiddenFor<AvailabilityRulesListResponse>("You are not allowed to view these rules.");
        }

        var rules = await _ruleRepository.GetByEmployeeIdAsync(organizationId, employeeId, cancellationToken);
        var items = rules.Select(rule => AvailabilityRuleMapper.ToResponse(rule)).ToList();
        return Result<AvailabilityRulesListResponse>.Success(new AvailabilityRulesListResponse(items));
    }

    public async Task<Result<PlanningAvailabilityResponse>> GetForPlanningAsync(
        PlanningAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<PlanningAvailabilityResponse>();
        }
        var employeeIds = await ResolveEmployeeIdsForPlanningAsync(request.EmployeeIds, organizationId, cancellationToken);
        if (employeeIds is null)
        {
            return Failures.ForbiddenFor<PlanningAvailabilityResponse>("You are not allowed to view this availability.");
        }

        var rules = await _ruleRepository.GetForPlanningAsync(
            organizationId,
            employeeIds,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        var periods = AvailabilityPeriodExpander.ExpandForRange(rules, request.StartDate, request.EndDate);
        return Result<PlanningAvailabilityResponse>.Success(new PlanningAvailabilityResponse(periods));
    }

    public async Task<Result<AvailabilityRuleResponse>> CreateAsync(
        CreateAvailabilityRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<AvailabilityRuleResponse>();
        }

        if (request.EmployeeId != CurrentUser.UserId && !CanManageAnyEmployee())
        {
            return Failures.ForbiddenFor<AvailabilityRuleResponse>("You are not allowed to create this rule.");
        }

        // Loaded once: it answers both "does this employee exist here" and "do their own entries
        // need approval".
        var employee = await LoadEmployeeAsync(request.EmployeeId, organizationId, cancellationToken);

        if (employee is null)
        {
            return Failures.NotFoundFor<AvailabilityRuleResponse>("Employee");
        }

        var approvalStatus = ResolveApprovalStatus(employee);

        return await TranslateDomainErrorsAsync(async () =>
        {
            var utcNow = DateTime.UtcNow;
            var rule = request.Type switch
            {
                AvailabilityRuleType.Weekly when request.Weekday is { } weekday =>
                    AvailabilityRule.CreateWeekly(
                        organizationId,
                        request.EmployeeId,
                        weekday,
                        request.StartTime,
                        request.EndTime,
                        request.Status,
                        request.Reason,
                        approvalStatus,
                        utcNow),
                AvailabilityRuleType.OneTime when request.Date is { } date =>
                    AvailabilityRule.CreateOneTime(
                        organizationId,
                        request.EmployeeId,
                        date,
                        request.StartTime,
                        request.EndTime,
                        request.Status,
                        request.Reason,
                        approvalStatus,
                        utcNow),
                _ => throw new ArgumentException("Invalid rule type or missing required fields."),
            };

            await _ruleRepository.AddAsync(rule, cancellationToken);

            var schedulingConflict = rule.Date is { } createdDate
                && await HasSchedulingConflictAsync(organizationId, rule.EmployeeId, createdDate, rule.StartTime, rule.EndTime, cancellationToken);

            return Result<AvailabilityRuleResponse>.Success(AvailabilityRuleMapper.ToResponse(rule, schedulingConflict));
        });
    }

    public async Task<Result<AvailabilityRuleResponse>> UpdateAsync(
        Guid id,
        UpdateAvailabilityRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<AvailabilityRuleResponse>();
        }
        var existing = await _ruleRepository.GetByIdAsync(id, cancellationToken);

        if (existing is null || !Owns(existing))
        {
            return Failures.NotFoundFor<AvailabilityRuleResponse>("Availability rule");
        }

        if (!await CanManageAnyEmployeeAsync(existing.EmployeeId, organizationId, cancellationToken))
        {
            return Failures.ForbiddenFor<AvailabilityRuleResponse>("You are not allowed to update this rule.");
        }

        return await TranslateDomainErrorsAsync(async () =>
        {
            var utcNow = DateTime.UtcNow;

            if (existing.Type == AvailabilityRuleType.Weekly)
            {
                if (request.Weekday is not { } weekday)
                {
                    return Result<AvailabilityRuleResponse>.Failure("Weekday is required for weekly rules.", Failures.Validation);
                }

                existing.UpdateWeekly(
                    weekday,
                    request.StartTime,
                    request.EndTime,
                    request.Status,
                    request.Reason,
                    utcNow);
            }
            else
            {
                if (request.Date is not { } date)
                {
                    return Result<AvailabilityRuleResponse>.Failure("Date is required for one-time rules.", Failures.Validation);
                }

                existing.UpdateOneTime(
                    date,
                    request.StartTime,
                    request.EndTime,
                    request.Status,
                    request.Reason,
                    utcNow);
            }

            await _ruleRepository.UpdateAsync(existing, cancellationToken);

            var schedulingConflict = existing.Date is { } updatedDate
                && await HasSchedulingConflictAsync(organizationId, existing.EmployeeId, updatedDate, existing.StartTime, existing.EndTime, cancellationToken);

            return Result<AvailabilityRuleResponse>.Success(AvailabilityRuleMapper.ToResponse(existing, schedulingConflict));
        });
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext();
        }
        var existing = await _ruleRepository.GetByIdAsync(id, cancellationToken);

        if (existing is null || !Owns(existing))
        {
            return Failures.NotFoundFor("Availability rule");
        }

        // Withdrawing your own request is allowed even though deleting is otherwise planner-only: a
        // pending rule was never granted, so removing it is not the end run around delete that
        // removing an approved one would be.
        var isOwnPendingRequest = existing.EmployeeId == CurrentUser.UserId
            && existing.ApprovalStatus == ApprovalStatus.Pending;

        if (!isOwnPendingRequest
            && !await CanManageAnyEmployeeAsync(existing.EmployeeId, organizationId, cancellationToken))
        {
            return Failures.ForbiddenFor("You are not allowed to delete this rule.");
        }

        await _ruleRepository.DeleteAsync(existing, cancellationToken);
        return Result.Success();
    }

    private async Task<IReadOnlyList<Guid>?> ResolveEmployeeIdsForPlanningAsync(
        string? employeeIds,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(employeeIds))
        {
            if (!CsvList.TryParseGuids(employeeIds, out var parsed) || parsed is null)
            {
                return null;
            }

            // Not just "does this employee exist" - rules carry a free-text reason, so the caller
            // must actually be allowed to see them. Resolved against one team query instead of a
            // round-trip per requested id.
            if (!CanManageAnyEmployee())
            {
                return parsed.All(id => id == CurrentUser.UserId) ? parsed : null;
            }

            var teamIds = (await _userRepository.GetByOrganizationIdAsync(organizationId, cancellationToken))
                .Select(x => x.Id)
                .ToHashSet();

            return parsed.All(teamIds.Contains) ? parsed : null;
        }

        // No filter given: a planner sees the whole team, anyone else only themselves.
        if (!CanManageAnyEmployee())
        {
            return CurrentUser.UserId is { } ownId ? new List<Guid> { ownId } : [];
        }

        var users = await _userRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
        return users.Where(x => x.IsActive).Select(x => x.Id).ToList();
    }

    /// <summary>
    /// A planner records an absence as a fact - they are the approver, including when they enter it
    /// for someone else. Only a member submitting for themselves can end up pending, and only when
    /// the organization flagged them as having to request it.
    /// </summary>
    private ApprovalStatus ResolveApprovalStatus(User employee) =>
        CanManageAnyEmployee() || !employee.RequiresApproval
            ? ApprovalStatus.Approved
            : ApprovalStatus.Pending;

    /// <summary>
    /// Reading and recording an absence is self-service: you may always do it for yourself.
    /// A planner may do it for anyone in the organization.
    /// </summary>
    private async Task<bool> CanManageForEmployeeAsync(
        Guid employeeId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (CurrentUser.UserId == employeeId)
        {
            return true;
        }

        return await CanManageAnyEmployeeAsync(employeeId, organizationId, cancellationToken);
    }

    /// <summary>
    /// Changing or removing an existing rule is planner-only, including your own: shrinking a rule
    /// to nothing would otherwise be an end run around not being allowed to delete it.
    /// </summary>
    private async Task<bool> CanManageAnyEmployeeAsync(
        Guid employeeId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (!CanManageAnyEmployee())
        {
            return false;
        }

        return await EmployeeExistsInOrganizationAsync(employeeId, organizationId, cancellationToken);
    }

    private bool CanManageAnyEmployee() =>
        CurrentUser.Role is UserRole.Owner or UserRole.Admin or UserRole.Planner;

    private async Task<bool> HasSchedulingConflictAsync(
        Guid organizationId,
        Guid employeeId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken)
    {
        var dayStartUtc = date.ToDateTime(TimeOnly.MinValue);
        var dayEndUtc = dayStartUtc.AddDays(1);

        var (records, _) = await _planningRecordRepository.GetByOrganizationAndRangeAsync(
            organizationId,
            dayStartUtc,
            dayEndUtc,
            new[] { employeeId },
            null,
            null,
            null,
            1,
            int.MaxValue,
            cancellationToken);

        var ruleStartUtc = date.ToDateTime(startTime);
        var ruleEndUtc = date.ToDateTime(endTime);

        return records.Any(record =>
            record.Status != PlanningStatus.Cancelled
            && record.StartUtc < ruleEndUtc
            && record.EndUtc > ruleStartUtc);
    }

    private async Task<bool> EmployeeExistsInOrganizationAsync(
        Guid employeeId,
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await LoadEmployeeAsync(employeeId, organizationId, cancellationToken) is not null;

    private async Task<User?> LoadEmployeeAsync(
        Guid employeeId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(employeeId, cancellationToken);
        return user is not null && user.OrganizationId == organizationId ? user : null;
    }
}
