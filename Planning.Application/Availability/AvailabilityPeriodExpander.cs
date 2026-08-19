using Planning.Domain.Availability;
using Planning.Domain.Enums;

namespace Planning.Application.Availability;

public static class AvailabilityPeriodExpander
{
    public static IReadOnlyList<UnavailablePeriodResponse> ExpandForDate(
        IReadOnlyList<AvailabilityRule> rules,
        Guid employeeId,
        DateOnly date)
    {
        var weekday = ToWeekday(date.DayOfWeek);
        var periods = new List<UnavailablePeriodResponse>();

        foreach (var rule in rules.Where(x => x.EmployeeId == employeeId))
        {
            if (rule.Type == AvailabilityRuleType.Weekly)
            {
                if (rule.Weekday != weekday)
                {
                    continue;
                }
            }
            else if (rule.Date != date)
            {
                continue;
            }

            periods.Add(new UnavailablePeriodResponse(
                employeeId,
                date,
                rule.StartTime,
                rule.EndTime,
                rule.Status,
                rule.Reason,
                rule.Id));
        }

        return periods;
    }

    /// <summary>
    /// Expands every rule into concrete periods for each day in the range.
    /// </summary>
    public static IReadOnlyList<UnavailablePeriodResponse> ExpandForRange(
        IReadOnlyList<AvailabilityRule> rules,
        DateOnly rangeStart,
        DateOnly rangeEnd)
    {
        // The set of employees does not change per day, so resolve it once instead of
        // re-deriving it inside the loop.
        var employeeIds = rules.Select(x => x.EmployeeId).Distinct().ToList();
        var periods = new List<UnavailablePeriodResponse>();

        for (var cursor = rangeStart; cursor <= rangeEnd; cursor = cursor.AddDays(1))
        {
            foreach (var employeeId in employeeIds)
            {
                periods.AddRange(ExpandForDate(rules, employeeId, cursor));
            }
        }

        return periods;
    }

    private static Weekday ToWeekday(DayOfWeek dayOfWeek) =>
        dayOfWeek switch
        {
            DayOfWeek.Monday => Weekday.Monday,
            DayOfWeek.Tuesday => Weekday.Tuesday,
            DayOfWeek.Wednesday => Weekday.Wednesday,
            DayOfWeek.Thursday => Weekday.Thursday,
            DayOfWeek.Friday => Weekday.Friday,
            DayOfWeek.Saturday => Weekday.Saturday,
            DayOfWeek.Sunday => Weekday.Sunday,
            _ => Weekday.Monday,
        };
}
