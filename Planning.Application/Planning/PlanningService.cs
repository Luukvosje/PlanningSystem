using Planning.Application.Common;
using Planning.Domain.Customers;
using Planning.Domain.Enums;
using Planning.Domain.Organizations;
using Planning.Domain.Planning;
using Planning.Domain.Users;

namespace Planning.Application.Planning;

public class PlanningService : TenantServiceBase, IPlanningService
{
    private const int MaxPageSize = 2000;

    private readonly IPlanningRecordRepository _planningRecordRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUserRepository _userRepository;

    public PlanningService(
        IPlanningRecordRepository planningRecordRepository,
        ICustomerRepository customerRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
        : base(currentUserContext)
    {
        _planningRecordRepository = planningRecordRepository;
        _customerRepository = customerRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<PlanningResponse>> CreateAsync(
        CreatePlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<PlanningResponse>();
        }

        var validationResult = await ValidateReferencesAsync(
            organizationId,
            request.CustomerId,
            request.AssignedUserId,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        return await TranslateDomainErrorsAsync(async () =>
        {
            var planningRecord = PlanningRecord.Create(
                organizationId,
                request.CustomerId,
                request.AssignedUserId,
                request.Title,
                request.Description,
                request.Notes,
                request.StartUtc,
                request.EndUtc,
                request.Color,
                DateTime.UtcNow,
                request.Status);

            await _planningRecordRepository.AddAsync(planningRecord, cancellationToken);
            return await BuildSingleResponseAsync(planningRecord, cancellationToken);
        });
    }

    public async Task<Result<PlanningResponse>> UpdateAsync(
        Guid id,
        UpdatePlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !Owns(planningRecord))
        {
            return Failures.NotFoundFor<PlanningResponse>("Planning record");
        }

        var validationResult = await ValidateReferencesAsync(
            planningRecord.OrganizationId,
            request.CustomerId,
            request.AssignedUserId,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        return await TranslateDomainErrorsAsync(async () =>
        {
            var utcNow = DateTime.UtcNow;

            planningRecord.Update(
                request.CustomerId,
                request.AssignedUserId,
                request.Title,
                request.Description,
                request.Notes,
                request.StartUtc,
                request.EndUtc,
                request.Color,
                utcNow);

            planningRecord.ChangeStatus(request.Status, utcNow);
            await _planningRecordRepository.UpdateAsync(planningRecord, cancellationToken);
            return await BuildSingleResponseAsync(planningRecord, cancellationToken);
        });
    }

    public async Task<Result<PlanningResponse>> MoveAsync(
        Guid id,
        MovePlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !Owns(planningRecord))
        {
            return Failures.NotFoundFor<PlanningResponse>("Planning record");
        }

        var validationResult = await ValidateReferencesAsync(
            planningRecord.OrganizationId,
            request.CustomerId,
            request.AssignedUserId,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        return await TranslateDomainErrorsAsync(async () =>
        {
            planningRecord.Move(
                request.AssignedUserId,
                request.CustomerId,
                request.StartUtc,
                request.EndUtc,
                DateTime.UtcNow);

            await _planningRecordRepository.UpdateAsync(planningRecord, cancellationToken);
            return await BuildSingleResponseAsync(planningRecord, cancellationToken);
        });
    }

    public async Task<Result<PlanningResponse>> ConfirmAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !Owns(planningRecord))
        {
            return Failures.NotFoundFor<PlanningResponse>("Planning record");
        }

        return await TranslateDomainErrorsAsync(async () =>
        {
            planningRecord.Confirm(DateTime.UtcNow);
            await _planningRecordRepository.UpdateAsync(planningRecord, cancellationToken);
            return await BuildSingleResponseAsync(planningRecord, cancellationToken);
        });
    }

    public async Task<Result<PlanningResponse>> DuplicateAsync(
        Guid id,
        DuplicatePlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !Owns(planningRecord))
        {
            return Failures.NotFoundFor<PlanningResponse>("Planning record");
        }

        var assignedUserId = request.AssignedUserId ?? planningRecord.AssignedUserId;

        var validationResult = await ValidateUserReferenceAsync(
            planningRecord.OrganizationId,
            assignedUserId,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        return await TranslateDomainErrorsAsync(async () =>
        {
            var duration = planningRecord.EndUtc - planningRecord.StartUtc;
            var startUtc = request.StartUtc ?? planningRecord.StartUtc;
            var endUtc = startUtc.Add(duration);

            var duplicate = planningRecord.Duplicate(assignedUserId, startUtc, endUtc, DateTime.UtcNow);
            await _planningRecordRepository.AddAsync(duplicate, cancellationToken);
            return await BuildSingleResponseAsync(duplicate, cancellationToken);
        });
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !Owns(planningRecord))
        {
            return Failures.NotFoundFor("Planning record");
        }

        await _planningRecordRepository.DeleteAsync(planningRecord, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PlanningResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !Owns(planningRecord))
        {
            return Failures.NotFoundFor<PlanningResponse>("Planning record");
        }

        return await BuildSingleResponseAsync(planningRecord, cancellationToken);
    }

    public async Task<Result<PlanningListResponse>> GetListAsync(
        PlanningListRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<PlanningListResponse>();
        }

        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var rangeStartUtc = request.StartUtc;
        var rangeEndUtc = request.EndUtc;

        if (rangeEndUtc <= rangeStartUtc)
        {
            return Result<PlanningListResponse>.Failure(
                "End date must be after start date.",
                Failures.Validation);
        }

        var (items, totalCount) = await _planningRecordRepository.GetByOrganizationAndRangeAsync(
            organizationId,
            rangeStartUtc,
            rangeEndUtc,
            CsvList.ParseGuidsOrNull(request.UserIds),
            CsvList.ParseGuidsOrNull(request.CustomerIds),
            CsvList.ParseEnumsOrNull<PlanningStatus>(request.Statuses),
            request.Search,
            request.Page,
            pageSize,
            cancellationToken);

        var responses = await BuildResponsesAsync(items, cancellationToken);

        return Result<PlanningListResponse>.Success(new PlanningListResponse(
            responses,
            totalCount,
            request.Page,
            pageSize,
            rangeStartUtc,
            rangeEndUtc));
    }

    private async Task<Result<PlanningResponse>> BuildSingleResponseAsync(
        PlanningRecord planningRecord,
        CancellationToken cancellationToken)
    {
        var responses = await BuildResponsesAsync([planningRecord], cancellationToken);
        return Result<PlanningResponse>.Success(responses[0]);
    }

    private async Task<IReadOnlyList<PlanningResponse>> BuildResponsesAsync(
        IReadOnlyList<PlanningRecord> records,
        CancellationToken cancellationToken)
    {
        if (records.Count == 0)
        {
            return [];
        }

        var organizationId = records[0].OrganizationId;
        var users = await _userRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
        var customers = await _customerRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);

        var userNames = users.ToDictionary(x => x.Id, x => $"{x.FirstName} {x.LastName}".Trim());
        var customerNames = customers.ToDictionary(x => x.Id, x => x.Name);

        var overlapLookup = BuildOverlapLookup(records);

        return records
            .Select(record => PlanningMapper.ToResponse(
                record,
                record.AssignedUserId.HasValue
                    ? userNames.GetValueOrDefault(record.AssignedUserId.Value, "Onbekend")
                    : null,
                record.CustomerId.HasValue
                    ? customerNames.GetValueOrDefault(record.CustomerId.Value)
                    : null,
                overlapLookup.GetValueOrDefault(record.Id, false)))
            .ToList();
    }

    private static Dictionary<Guid, bool> BuildOverlapLookup(IReadOnlyList<PlanningRecord> records)
    {
        var lookup = records.ToDictionary(x => x.Id, _ => false);

        // A cancelled booking no longer occupies the employee, so it neither overlaps nor causes
        // one. Same rule as AvailabilityRuleService.HasSchedulingConflictAsync.
        // Open shifts are excluded as well: with no employee attached they cannot occupy anyone,
        // so two open shifts at the same time are not a conflict.
        var byUser = records
            .Where(x => x.Status != PlanningStatus.Cancelled && x.AssignedUserId.HasValue)
            .GroupBy(x => x.AssignedUserId!.Value);

        foreach (var group in byUser)
        {
            var userRecords = group.OrderBy(x => x.StartUtc).ToList();

            for (var i = 0; i < userRecords.Count; i++)
            {
                for (var j = i + 1; j < userRecords.Count; j++)
                {
                    if (userRecords[j].StartUtc >= userRecords[i].EndUtc)
                    {
                        break;
                    }

                    lookup[userRecords[i].Id] = true;
                    lookup[userRecords[j].Id] = true;
                }
            }
        }

        return lookup;
    }

    private async Task<Result<PlanningResponse>?> ValidateReferencesAsync(
        Guid organizationId,
        Guid? customerId,
        Guid? assignedUserId,
        CancellationToken cancellationToken)
    {
        if (customerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId.Value, cancellationToken);
            if (customer is null || customer.OrganizationId != organizationId)
            {
                return Result<PlanningResponse>.Failure("Customer not found for organization.", Failures.NotFound);
            }
        }

        return await ValidateUserReferenceAsync(organizationId, assignedUserId, cancellationToken);
    }

    /// <summary>
    /// Returns null when the reference is fine. An unassigned shift is an open shift, so there is
    /// nothing to check — only a supplied user has to exist inside the same organization.
    /// </summary>
    private async Task<Result<PlanningResponse>?> ValidateUserReferenceAsync(
        Guid organizationId,
        Guid? assignedUserId,
        CancellationToken cancellationToken)
    {
        if (!assignedUserId.HasValue)
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(assignedUserId.Value, cancellationToken);
        if (user is null || user.OrganizationId != organizationId)
        {
            return Result<PlanningResponse>.Failure("Assigned user not found for organization.", Failures.NotFound);
        }

        return null;
    }

}
