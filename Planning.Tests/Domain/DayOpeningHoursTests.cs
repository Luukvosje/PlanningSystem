using Planning.Domain.Enums;
using Planning.Domain.Organizations;

namespace Planning.Tests.Domain;

public class DayOpeningHoursTests
{
    [Fact]
    public void Accepts_an_open_time_before_the_close_time()
    {
        var hours = new DayOpeningHours(Weekday.Monday, new TimeOnly(6, 0), new TimeOnly(22, 0));

        Assert.Equal(Weekday.Monday, hours.Day);
        Assert.Equal(new TimeOnly(6, 0), hours.OpenTime);
        Assert.Equal(new TimeOnly(22, 0), hours.CloseTime);
    }

    [Fact]
    public void Rejects_an_open_time_equal_to_the_close_time() =>
        Assert.Throws<ArgumentException>(() =>
            new DayOpeningHours(Weekday.Monday, new TimeOnly(9, 0), new TimeOnly(9, 0)));

    [Fact]
    public void Rejects_an_open_time_after_the_close_time() =>
        Assert.Throws<ArgumentException>(() =>
            new DayOpeningHours(Weekday.Monday, new TimeOnly(22, 0), new TimeOnly(6, 0)));

    /// <summary>
    /// A 24-hour day is not representable: 00:00-00:00 violates open &lt; close.
    /// The frontend encodes "whole day" as 23:59, see types/availability.ts.
    /// </summary>
    [Fact]
    public void Cannot_express_a_full_24_hour_day() =>
        Assert.Throws<ArgumentException>(() =>
            new DayOpeningHours(Weekday.Monday, TimeOnly.MinValue, TimeOnly.MinValue));
}
