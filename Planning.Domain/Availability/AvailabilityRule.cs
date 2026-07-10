using Planning.Domain.Common;
using Planning.Domain.Enums;

namespace Planning.Domain.Availability;

public class AvailabilityRule : TenantEntity
{
    public Guid EmployeeId { get; private set; }
    public AvailabilityRuleType Type { get; private set; }
    public Weekday? Weekday { get; private set; }
    public DateOnly? Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public AvailabilityRuleStatus Status { get; private set; }
    public string? Reason { get; private set; }

    private AvailabilityRule()
    {
    }

    private AvailabilityRule(
        Guid id,
        Guid organizationId,
        Guid employeeId,
        AvailabilityRuleType type,
        Weekday? weekday,
        DateOnly? date,
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilityRuleStatus status,
        string? reason,
        DateTime utcNow)
        : base(id, organizationId, utcNow, utcNow)
    {
        EmployeeId = employeeId;
        Type = type;
        Weekday = weekday;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Status = status;
        Reason = reason?.Trim();
    }

    public static AvailabilityRule CreateWeekly(
        Guid organizationId,
        Guid employeeId,
        Weekday weekday,
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilityRuleStatus status,
        string? reason,
        DateTime utcNow)
    {
        ValidateWeekly(weekday, startTime, endTime, status);

        return new AvailabilityRule(
            Guid.NewGuid(),
            organizationId,
            employeeId,
            AvailabilityRuleType.Weekly,
            weekday,
            null,
            startTime,
            endTime,
            status,
            reason,
            utcNow);
    }

    public static AvailabilityRule CreateOneTime(
        Guid organizationId,
        Guid employeeId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilityRuleStatus status,
        string? reason,
        DateTime utcNow)
    {
        ValidateOneTime(date, startTime, endTime, status);

        return new AvailabilityRule(
            Guid.NewGuid(),
            organizationId,
            employeeId,
            AvailabilityRuleType.OneTime,
            null,
            date,
            startTime,
            endTime,
            status,
            reason,
            utcNow);
    }

    public void UpdateWeekly(
        Weekday weekday,
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilityRuleStatus status,
        string? reason,
        DateTime utcNow)
    {
        if (Type != AvailabilityRuleType.Weekly)
        {
            throw new InvalidOperationException("Cannot update weekly fields on a one-time rule.");
        }

        ValidateWeekly(weekday, startTime, endTime, status);

        Weekday = weekday;
        StartTime = startTime;
        EndTime = endTime;
        Status = status;
        Reason = reason?.Trim();
        Touch(utcNow);
    }

    public void UpdateOneTime(
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilityRuleStatus status,
        string? reason,
        DateTime utcNow)
    {
        if (Type != AvailabilityRuleType.OneTime)
        {
            throw new InvalidOperationException("Cannot update one-time fields on a weekly rule.");
        }

        ValidateOneTime(date, startTime, endTime, status);

        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Status = status;
        Reason = reason?.Trim();
        Touch(utcNow);
    }

    private static void ValidateWeekly(
        Weekday weekday,
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilityRuleStatus status)
    {
        _ = weekday;
        ValidateTimeRange(startTime, endTime);
        ValidateStatus(status);
    }

    private static void ValidateOneTime(
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilityRuleStatus status)
    {
        _ = date;
        ValidateTimeRange(startTime, endTime);
        ValidateStatus(status);
    }

    private static void ValidateTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("End time must be after start time.");
        }
    }

    private static void ValidateStatus(AvailabilityRuleStatus status)
    {
        if (status != AvailabilityRuleStatus.Unavailable)
        {
            throw new ArgumentException("Only unavailable rules are supported at this time.");
        }
    }
}
