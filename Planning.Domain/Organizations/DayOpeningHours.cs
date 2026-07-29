using Planning.Domain.Enums;

namespace Planning.Domain.Organizations;

public sealed class DayOpeningHours
{
    public Weekday Day { get; init; }
    public TimeOnly OpenTime { get; init; }
    public TimeOnly CloseTime { get; init; }

    private DayOpeningHours()
    {
    }

    public DayOpeningHours(Weekday day, TimeOnly openTime, TimeOnly closeTime)
    {
        if (openTime >= closeTime)
        {
            throw new ArgumentException("Open time must be before close time.");
        }

        Day = day;
        OpenTime = openTime;
        CloseTime = closeTime;
    }
}
