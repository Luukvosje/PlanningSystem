using Planning.Domain.Availability;
using Planning.Domain.Enums;
using Planning.Domain.Users;

namespace Planning.Application.Requests;

internal static class RequestMapper
{
    public static RequestResponse ToResponse(AvailabilityRule rule, User? employee) =>
        new(
            rule.Id,
            ToKind(rule.Type),
            rule.EmployeeId,
            employee?.FirstName ?? string.Empty,
            employee?.LastName ?? string.Empty,
            rule.ApprovalStatus,
            rule.Date,
            rule.Weekday,
            rule.StartTime,
            rule.EndTime,
            rule.Reason,
            rule.CreatedAtUtc,
            rule.DecidedAtUtc);

    /// <summary>
    /// A dated absence is leave ("I am off on the 25th"); a weekly block is a change to the
    /// standing availability ("never on Monday mornings"). Same table, two things to decide on.
    /// </summary>
    private static RequestKind ToKind(AvailabilityRuleType type) =>
        type == AvailabilityRuleType.OneTime ? RequestKind.Leave : RequestKind.Availability;
}
