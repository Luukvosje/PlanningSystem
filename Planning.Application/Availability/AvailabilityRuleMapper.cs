using Planning.Domain.Availability;

namespace Planning.Application.Availability;

public static class AvailabilityRuleMapper
{
    public static AvailabilityRuleResponse ToResponse(AvailabilityRule rule, bool schedulingConflict = false) =>
        new(
            rule.Id,
            rule.EmployeeId,
            rule.Type,
            rule.Weekday,
            rule.Date,
            rule.StartTime,
            rule.EndTime,
            rule.Status,
            rule.Reason,
            rule.ApprovalStatus,
            rule.DecidedAtUtc,
            rule.CreatedAtUtc,
            rule.UpdatedAtUtc,
            schedulingConflict);
}
