using Planning.Domain.Availability;
using Planning.Domain.Enums;

namespace Planning.Application.Availability;

public static class AvailabilityOverlapChecker
{
    public static bool HasOverlap(
        IReadOnlyList<AvailabilityRule> rules,
        Guid employeeId,
        DateTime shiftStartUtc,
        DateTime shiftEndUtc)
    {
        var shiftStart = shiftStartUtc;
        var shiftEnd = shiftEndUtc;
        var date = DateOnly.FromDateTime(shiftStart.Date);

        foreach (var period in AvailabilityPeriodExpander.ExpandForDate(rules, employeeId, date))
        {
            if (period.Status != AvailabilityRuleStatus.Unavailable)
            {
                continue;
            }

            var periodStart = period.Date.ToDateTime(period.StartTime);
            var periodEnd = period.Date.ToDateTime(period.EndTime);

            if (shiftStart < periodEnd && shiftEnd > periodStart)
            {
                return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<UnavailablePeriodResponse> ExpandForRange(
        IReadOnlyList<AvailabilityRule> rules,
        DateOnly rangeStart,
        DateOnly rangeEnd)
    {
        var periods = new List<UnavailablePeriodResponse>();
        var cursor = rangeStart;

        while (cursor <= rangeEnd)
        {
            var employeeIds = rules.Select(x => x.EmployeeId).Distinct();
            foreach (var employeeId in employeeIds)
            {
                periods.AddRange(
                    AvailabilityPeriodExpander.ExpandForDate(rules, employeeId, cursor));
            }

            cursor = cursor.AddDays(1);
        }

        return periods;
    }
}
