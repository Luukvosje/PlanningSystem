using Planning.Domain.Common;
using Planning.Domain.Enums;

namespace Planning.Domain.Availability;

public class EmployeeAvailability : TenantEntity
{
    public Guid UserId { get; private set; }
    public DateOnly Date { get; private set; }
    public AvailabilityType Type { get; private set; }
    public DayPart? DayPart { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public bool IsAvailable { get; private set; }
    public AvailabilitySource Source { get; private set; }
    public Guid LastModifiedByUserId { get; private set; }
    public string? Note { get; private set; }

    private EmployeeAvailability()
    {
    }

    private EmployeeAvailability(
        Guid id,
        Guid organizationId,
        Guid userId,
        DateOnly date,
        AvailabilityType type,
        DayPart? dayPart,
        TimeOnly? startTime,
        TimeOnly? endTime,
        bool isAvailable,
        AvailabilitySource source,
        Guid lastModifiedByUserId,
        string? note,
        DateTime utcNow)
        : base(id, organizationId, utcNow, utcNow)
    {
        UserId = userId;
        Date = date;
        Type = type;
        DayPart = dayPart;
        StartTime = startTime;
        EndTime = endTime;
        IsAvailable = isAvailable;
        Source = source;
        LastModifiedByUserId = lastModifiedByUserId;
        Note = note?.Trim();
    }

    public static EmployeeAvailability CreateDayPart(
        Guid organizationId,
        Guid userId,
        DateOnly date,
        DayPart dayPart,
        bool isAvailable,
        AvailabilitySource source,
        Guid lastModifiedByUserId,
        string? note,
        DateTime utcNow)
    {
        ValidateDayPart(isAvailable);

        return new EmployeeAvailability(
            Guid.NewGuid(),
            organizationId,
            userId,
            date,
            AvailabilityType.DayPart,
            dayPart,
            null,
            null,
            isAvailable,
            source,
            lastModifiedByUserId,
            note,
            utcNow);
    }

    public static EmployeeAvailability CreateTimeBlock(
        Guid organizationId,
        Guid userId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilitySource source,
        Guid lastModifiedByUserId,
        string? note,
        DateTime utcNow)
    {
        ValidateTimeBlock(startTime, endTime);

        return new EmployeeAvailability(
            Guid.NewGuid(),
            organizationId,
            userId,
            date,
            AvailabilityType.TimeBlock,
            null,
            startTime,
            endTime,
            false,
            source,
            lastModifiedByUserId,
            note,
            utcNow);
    }

    public void UpdateDayPart(
        bool isAvailable,
        AvailabilitySource source,
        Guid lastModifiedByUserId,
        string? note,
        DateTime utcNow)
    {
        if (Type != AvailabilityType.DayPart)
        {
            throw new InvalidOperationException("Cannot update day part on a time block entry.");
        }

        ValidateDayPart(isAvailable);

        IsAvailable = isAvailable;
        Source = source;
        LastModifiedByUserId = lastModifiedByUserId;
        Note = note?.Trim();
        Touch(utcNow);
    }

    public void UpdateTimeBlock(
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilitySource source,
        Guid lastModifiedByUserId,
        string? note,
        DateTime utcNow)
    {
        if (Type != AvailabilityType.TimeBlock)
        {
            throw new InvalidOperationException("Cannot update time block on a day part entry.");
        }

        ValidateTimeBlock(startTime, endTime);

        StartTime = startTime;
        EndTime = endTime;
        Source = source;
        LastModifiedByUserId = lastModifiedByUserId;
        Note = note?.Trim();
        Touch(utcNow);
    }

    private static void ValidateDayPart(bool isAvailable)
    {
        if (isAvailable)
        {
            throw new ArgumentException("Day part entries only store unavailability. Use delete to mark available.");
        }
    }

    private static void ValidateTimeBlock(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("End time must be after start time.");
        }
    }
}
