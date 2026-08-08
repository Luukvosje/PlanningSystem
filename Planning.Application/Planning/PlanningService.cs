using Planning.Application.Common;
using Planning.Domain.Customers;
using Planning.Domain.Enums;
using Planning.Domain.Organizations;
using Planning.Domain.Planning;
using Planning.Domain.Users;

namespace Planning.Application.Planning;

public class PlanningService : IPlanningService
{
    private const int MaxPageSize = 2000;

    private readonly IPlanningRecordRepository _planningRecordRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public PlanningService(
        IPlanningRecordRepository planningRecordRepository,
        ICustomerRepository customerRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
    {
        _planningRecordRepository = planningRecordRepository;
        _customerRepository = customerRepository;
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<Result<PlanningResponse>> CreateAsync(
        CreatePlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<PlanningResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;

        var validationResult = await ValidateReferencesAsync(
            organizationId,
            request.CustomerId,
            request.AssignedUserId,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        try
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
        }
        catch (ArgumentException ex)
        {
            return Result<PlanningResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result<PlanningResponse>> UpdateAsync(
        Guid id,
        UpdatePlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !BelongsToCurrentOrganization(planningRecord))
        {
            return Result<PlanningResponse>.Failure("Planning record not found.", "NOT_FOUND");
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

        try
        {
            planningRecord.Update(
                request.CustomerId,
                request.AssignedUserId,
                request.Title,
                request.Description,
                request.Notes,
                request.StartUtc,
                request.EndUtc,
                request.Color,
                DateTime.UtcNow);

            planningRecord.ChangeStatus(request.Status, DateTime.UtcNow);
            await _planningRecordRepository.UpdateAsync(planningRecord, cancellationToken);
            return await BuildSingleResponseAsync(planningRecord, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return Result<PlanningResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result<PlanningResponse>> MoveAsync(
        Guid id,
        MovePlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !BelongsToCurrentOrganization(planningRecord))
        {
            return Result<PlanningResponse>.Failure("Planning record not found.", "NOT_FOUND");
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

        try
        {
            planningRecord.Move(
                request.AssignedUserId,
                request.CustomerId,
                request.StartUtc,
                request.EndUtc,
                DateTime.UtcNow);

            await _planningRecordRepository.UpdateAsync(planningRecord, cancellationToken);
            return await BuildSingleResponseAsync(planningRecord, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return Result<PlanningResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result<PlanningResponse>> DuplicateAsync(
        Guid id,
        DuplicatePlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !BelongsToCurrentOrganization(planningRecord))
        {
            return Result<PlanningResponse>.Failure("Planning record not found.", "NOT_FOUND");
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

        try
        {
            var duration = planningRecord.EndUtc - planningRecord.StartUtc;
            var startUtc = request.StartUtc ?? planningRecord.StartUtc;
            var endUtc = startUtc.Add(duration);

            var duplicate = planningRecord.Duplicate(assignedUserId, startUtc, endUtc, DateTime.UtcNow);
            await _planningRecordRepository.AddAsync(duplicate, cancellationToken);
            return await BuildSingleResponseAsync(duplicate, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return Result<PlanningResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !BelongsToCurrentOrganization(planningRecord))
        {
            return Result.Failure("Planning record not found.", "NOT_FOUND");
        }

        await _planningRecordRepository.DeleteAsync(planningRecord, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PlanningResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var planningRecord = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

        if (planningRecord is null || !BelongsToCurrentOrganization(planningRecord))
        {
            return Result<PlanningResponse>.Failure("Planning record not found.", "NOT_FOUND");
        }

        return await BuildSingleResponseAsync(planningRecord, cancellationToken);
    }

    public async Task<Result<PlanningListResponse>> GetListAsync(
        PlanningListRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<PlanningListResponse>.Failure(
                "Organization context is required.",
                "NO_ORGANIZATION");
        }

        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var rangeStartUtc = request.StartUtc;
        var rangeEndUtc = request.EndUtc;

        if (rangeEndUtc <= rangeStartUtc)
        {
            return Result<PlanningListResponse>.Failure(
                "End date must be after start date.",
                "VALIDATION_ERROR");
        }

        var (items, totalCount) = await _planningRecordRepository.GetByOrganizationAndRangeAsync(
            _currentUserContext.OrganizationId!.Value,
            rangeStartUtc,
            rangeEndUtc,
            ParseGuidList(request.UserIds),
            ParseGuidList(request.CustomerIds),
            ParseStatusList(request.Statuses),
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

    public async Task<Result<IReadOnlyList<PlanningResponse>>> GetWeekPlanningAsync(
        WeekPlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<IReadOnlyList<PlanningResponse>>.Failure(
                "Organization context is required.",
                "NO_ORGANIZATION");
        }

        var weekStartUtc = request.WeekStartUtc.Date;
        var weekEndUtc = weekStartUtc.AddDays(7);

        var records = await _planningRecordRepository.GetByOrganizationAndWeekAsync(
            _currentUserContext.OrganizationId!.Value,
            weekStartUtc,
            weekEndUtc,
            cancellationToken);

        var responses = await BuildResponsesAsync(records, cancellationToken);
        return Result<IReadOnlyList<PlanningResponse>>.Success(responses);
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
                userNames.GetValueOrDefault(record.AssignedUserId, "Onbekend"),
                record.CustomerId.HasValue
                    ? customerNames.GetValueOrDefault(record.CustomerId.Value)
                    : null,
                overlapLookup.GetValueOrDefault(record.Id, false)))
            .ToList();
    }

    private static Dictionary<Guid, bool> BuildOverlapLookup(IReadOnlyList<PlanningRecord> records)
    {
        var lookup = records.ToDictionary(x => x.Id, _ => false);

        var byUser = records.GroupBy(x => x.AssignedUserId);

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

    private bool BelongsToCurrentOrganization(PlanningRecord planningRecord) =>
        _currentUserContext.HasOrganization &&
        planningRecord.OrganizationId == _currentUserContext.OrganizationId;

    private async Task<Result<PlanningResponse>?> ValidateReferencesAsync(
        Guid organizationId,
        Guid? customerId,
        Guid assignedUserId,
        CancellationToken cancellationToken)
    {
        if (customerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId.Value, cancellationToken);
            if (customer is null || customer.OrganizationId != organizationId)
            {
                return Result<PlanningResponse>.Failure("Customer not found for organization.", "NOT_FOUND");
            }
        }

        return await ValidateUserReferenceAsync(organizationId, assignedUserId, cancellationToken);
    }

    private async Task<Result<PlanningResponse>?> ValidateUserReferenceAsync(
        Guid organizationId,
        Guid assignedUserId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(assignedUserId, cancellationToken);
        if (user is null || user.OrganizationId != organizationId)
        {
            return Result<PlanningResponse>.Failure("Assigned user not found for organization.", "NOT_FOUND");
        }

        return null;
    }

    private static IReadOnlyList<Guid>? ParseGuidList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Guid.Parse)
            .ToList();
    }

    private static IReadOnlyList<PlanningStatus>? ParseStatusList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Enum.Parse<PlanningStatus>)
            .ToList();
    }
}
