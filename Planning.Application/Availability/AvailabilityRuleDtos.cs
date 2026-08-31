using Planning.Domain.Enums;

namespace Planning.Application.Availability;

public sealed record CreateAvailabilityRuleRequest(
    Guid EmployeeId,
    AvailabilityRuleType Type,
    Weekday? Weekday,
    DateOnly? Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    AvailabilityRuleStatus Status,
    string? Reason);

public sealed record UpdateAvailabilityRuleRequest(
    Weekday? Weekday,
    DateOnly? Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    AvailabilityRuleStatus Status,
    string? Reason);

public sealed record AvailabilityRuleResponse(
    Guid Id,
    Guid EmployeeId,
    AvailabilityRuleType Type,
    Weekday? Weekday,
    DateOnly? Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    AvailabilityRuleStatus Status,
    string? Reason,
    ApprovalStatus ApprovalStatus,
    DateTime? DecidedAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    bool SchedulingConflict = false);

public sealed record AvailabilityRulesListResponse(
    IReadOnlyList<AvailabilityRuleResponse> Items);

public sealed record PlanningAvailabilityRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    string? EmployeeIds);

public sealed record UnavailablePeriodResponse(
    Guid EmployeeId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    AvailabilityRuleStatus Status,
    ApprovalStatus ApprovalStatus,
    string? Reason,
    Guid RuleId);

public sealed record PlanningAvailabilityResponse(
    IReadOnlyList<UnavailablePeriodResponse> Periods);
