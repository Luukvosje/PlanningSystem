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
