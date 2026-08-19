using Planning.Domain.Enums;
using Planning.Domain.Planning;

namespace Planning.Tests.Domain;

/// <summary>
/// Allowed transitions: Planned -> Confirmed/Cancelled, Confirmed -> Completed/Cancelled.
/// Completed and Cancelled are terminal. Setting the status it already has is a no-op.
/// </summary>
public class PlanningRecordStatusTests
{
    private static readonly DateTime Now = new(2026, 8, 18, 10, 0, 0, DateTimeKind.Utc);

    private static PlanningRecord Record(PlanningStatus status) =>
        PlanningRecord.Create(
            Guid.NewGuid(), null, Guid.NewGuid(), "Onderhoud", null, null,
            Now, Now.AddHours(1), null, Now, status);

    [Theory]
    [InlineData(PlanningStatus.Planned, PlanningStatus.Confirmed)]
    [InlineData(PlanningStatus.Planned, PlanningStatus.Cancelled)]
    [InlineData(PlanningStatus.Confirmed, PlanningStatus.Completed)]
    [InlineData(PlanningStatus.Confirmed, PlanningStatus.Cancelled)]
    public void Allows_a_forward_transition(PlanningStatus from, PlanningStatus to)
    {
        var record = Record(from);
        var later = Now.AddMinutes(5);

        record.ChangeStatus(to, later);

        Assert.Equal(to, record.Status);
        Assert.Equal(later, record.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(PlanningStatus.Planned, PlanningStatus.Completed)]
    [InlineData(PlanningStatus.Confirmed, PlanningStatus.Planned)]
    [InlineData(PlanningStatus.Completed, PlanningStatus.Confirmed)]
    [InlineData(PlanningStatus.Completed, PlanningStatus.Planned)]
    [InlineData(PlanningStatus.Completed, PlanningStatus.Cancelled)]
    [InlineData(PlanningStatus.Cancelled, PlanningStatus.Confirmed)]
    [InlineData(PlanningStatus.Cancelled, PlanningStatus.Planned)]
    [InlineData(PlanningStatus.Cancelled, PlanningStatus.Completed)]
    public void Refuses_a_transition_that_is_not_allowed(PlanningStatus from, PlanningStatus to)
    {
        var record = Record(from);

        Assert.Throws<ArgumentException>(() => record.ChangeStatus(to, Now.AddMinutes(5)));
        Assert.Equal(from, record.Status);
    }

    [Theory]
    [InlineData(PlanningStatus.Planned)]
    [InlineData(PlanningStatus.Confirmed)]
    [InlineData(PlanningStatus.Completed)]
    [InlineData(PlanningStatus.Cancelled)]
    public void Setting_the_current_status_is_a_no_op(PlanningStatus status)
    {
        var record = Record(status);

        record.ChangeStatus(status, Now.AddMinutes(5));

        Assert.Equal(status, record.Status);
        Assert.Equal(Now, record.UpdatedAtUtc);
    }

    [Fact]
    public void Confirm_promotes_a_planned_booking()
    {
        var record = Record(PlanningStatus.Planned);

        record.Confirm(Now.AddMinutes(5));

        Assert.Equal(PlanningStatus.Confirmed, record.Status);
    }

    [Fact]
    public void Confirming_an_already_confirmed_booking_is_harmless()
    {
        var record = Record(PlanningStatus.Confirmed);

        record.Confirm(Now.AddMinutes(5));

        Assert.Equal(PlanningStatus.Confirmed, record.Status);
    }

    [Theory]
    [InlineData(PlanningStatus.Completed)]
    [InlineData(PlanningStatus.Cancelled)]
    public void Cannot_confirm_a_booking_that_already_ended(PlanningStatus status)
    {
        var record = Record(status);

        Assert.Throws<ArgumentException>(() => record.Confirm(Now.AddMinutes(5)));
    }
}
