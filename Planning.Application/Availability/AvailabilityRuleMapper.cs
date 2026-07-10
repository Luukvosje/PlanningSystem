using Planning.Domain.Availability;

namespace Planning.Application.Availability;

public static class AvailabilityRuleMapper
{
    public static AvailabilityRuleResponse ToResponse(AvailabilityRule rule) =>
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
            rule.CreatedAtUtc,
            rule.UpdatedAtUtc);
}
