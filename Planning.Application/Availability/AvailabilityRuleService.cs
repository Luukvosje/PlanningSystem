using Planning.Application.Common;
using Planning.Domain.Availability;
using Planning.Domain.Enums;
using Planning.Domain.Planning;
using Planning.Domain.Users;

namespace Planning.Application.Availability;

public class AvailabilityRuleService : IAvailabilityRuleService
{
    private readonly IAvailabilityRuleRepository _ruleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPlanningRecordRepository _planningRecordRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public AvailabilityRuleService(
        IAvailabilityRuleRepository ruleRepository,
        IUserRepository userRepository,
        IPlanningRecordRepository planningRecordRepository,
        ICurrentUserContext currentUserContext)
    {
        _ruleRepository = ruleRepository;
        _userRepository = userRepository;
        _planningRecordRepository = planningRecordRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<Result<AvailabilityRulesListResponse>> ListByEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<AvailabilityRulesListResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;

        if (!await CanManageForEmployeeAsync(employeeId, organizationId, cancellationToken))
        {
            return Result<AvailabilityRulesListResponse>.Failure("You are not allowed to view these rules.", "FORBIDDEN");
        }

        var rules = await _ruleRepository.GetByEmployeeIdAsync(organizationId, employeeId, cancellationToken);
        var items = rules.Select(rule => AvailabilityRuleMapper.ToResponse(rule)).ToList();
        return Result<AvailabilityRulesListResponse>.Success(new AvailabilityRulesListResponse(items));
    }

    public async Task<Result<PlanningAvailabilityResponse>> GetForPlanningAsync(
        PlanningAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<PlanningAvailabilityResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;
        var employeeIds = await ResolveEmployeeIdsForPlanningAsync(request.EmployeeIds, organizationId, cancellationToken);
        if (employeeIds is null)
        {
            return Result<PlanningAvailabilityResponse>.Failure("You are not allowed to view this availability.", "FORBIDDEN");
        }

        var rules = await _ruleRepository.GetForPlanningAsync(
            organizationId,
            employeeIds,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        var periods = AvailabilityOverlapChecker.ExpandForRange(rules, request.StartDate, request.EndDate);
        return Result<PlanningAvailabilityResponse>.Success(new PlanningAvailabilityResponse(periods));
    }

    public async Task<Result<AvailabilityRuleResponse>> CreateAsync(
        CreateAvailabilityRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<AvailabilityRuleResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;

        if (!await CanManageForEmployeeAsync(request.EmployeeId, organizationId, cancellationToken))
        {
            return Result<AvailabilityRuleResponse>.Failure("You are not allowed to create this rule.", "FORBIDDEN");
        }

        if (!await EmployeeExistsInOrganizationAsync(request.EmployeeId, organizationId, cancellationToken))
        {
            return Result<AvailabilityRuleResponse>.Failure("Employee not found.", "NOT_FOUND");
        }

        try
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
                        utcNow),
                _ => throw new ArgumentException("Invalid rule type or missing required fields."),
            };

            await _ruleRepository.AddAsync(rule, cancellationToken);

            var schedulingConflict = rule.Date is { } createdDate
                && await HasSchedulingConflictAsync(organizationId, rule.EmployeeId, createdDate, rule.StartTime, rule.EndTime, cancellationToken);

            return Result<AvailabilityRuleResponse>.Success(AvailabilityRuleMapper.ToResponse(rule, schedulingConflict));
        }
        catch (ArgumentException ex)
        {
            return Result<AvailabilityRuleResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result<AvailabilityRuleResponse>> UpdateAsync(
        Guid id,
        UpdateAvailabilityRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<AvailabilityRuleResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;
        var existing = await _ruleRepository.GetByIdAsync(id, cancellationToken);

        if (existing is null || existing.OrganizationId != organizationId)
        {
            return Result<AvailabilityRuleResponse>.Failure("Availability rule not found.", "NOT_FOUND");
        }

        if (!await CanManageForEmployeeAsync(existing.EmployeeId, organizationId, cancellationToken))
        {
            return Result<AvailabilityRuleResponse>.Failure("You are not allowed to update this rule.", "FORBIDDEN");
        }

        try
        {
            var utcNow = DateTime.UtcNow;

            if (existing.Type == AvailabilityRuleType.Weekly)
            {
                if (request.Weekday is not { } weekday)
                {
                    return Result<AvailabilityRuleResponse>.Failure("Weekday is required for weekly rules.", "VALIDATION_ERROR");
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
                    return Result<AvailabilityRuleResponse>.Failure("Date is required for one-time rules.", "VALIDATION_ERROR");
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
        }
        catch (ArgumentException ex)
        {
            return Result<AvailabilityRuleResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;
        var existing = await _ruleRepository.GetByIdAsync(id, cancellationToken);

        if (existing is null || existing.OrganizationId != organizationId)
        {
            return Result.Failure("Availability rule not found.", "NOT_FOUND");
        }

        if (!await CanManageForEmployeeAsync(existing.EmployeeId, organizationId, cancellationToken))
        {
            return Result.Failure("You are not allowed to delete this rule.", "FORBIDDEN");
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
            var parsed = employeeIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Guid.Parse)
                .ToList();

            foreach (var employeeId in parsed)
            {
                if (!await EmployeeExistsInOrganizationAsync(employeeId, organizationId, cancellationToken))
                {
                    return null;
                }
            }

            return parsed;
        }

        var users = await _userRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
        return users.Where(x => x.IsActive).Select(x => x.Id).ToList();
    }

    private async Task<bool> CanManageForEmployeeAsync(
        Guid employeeId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (_currentUserContext.UserId == employeeId)
        {
            return true;
        }

        if (!CanManageAnyEmployee())
        {
            return false;
        }

        return await EmployeeExistsInOrganizationAsync(employeeId, organizationId, cancellationToken);
    }

    private bool CanManageAnyEmployee() =>
        _currentUserContext.Role is UserRole.Owner or UserRole.Admin or UserRole.Planner;

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
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(employeeId, cancellationToken);
        return user is not null && user.OrganizationId == organizationId;
    }
}
