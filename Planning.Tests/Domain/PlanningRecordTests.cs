using Planning.Domain.Enums;
using Planning.Domain.Planning;

namespace Planning.Tests.Domain;

public class PlanningRecordTests
{
    private static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AssignedUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTime Now = new(2026, 8, 18, 10, 0, 0, DateTimeKind.Utc);

    private static PlanningRecord Create(
        DateTime? startUtc = null,
        DateTime? endUtc = null,
        string title = "Onderhoud",
        string? color = null,
        PlanningStatus status = PlanningStatus.Confirmed) =>
        PlanningRecord.Create(
            OrganizationId,
            customerId: null,
            AssignedUserId,
            title,
            description: null,
            notes: null,
            startUtc ?? Now,
            endUtc ?? Now.AddHours(1),
            color,
            Now,
            status);

    [Fact]
    public void Create_trims_title_and_stamps_organization()
    {
        var record = Create(title: "  Onderhoud  ");

        Assert.Equal("Onderhoud", record.Title);
        Assert.Equal(OrganizationId, record.OrganizationId);
        Assert.Equal(Now, record.CreatedAtUtc);
        Assert.Equal(Now, record.UpdatedAtUtc);
    }

    [Fact]
    public void Create_defaults_to_confirmed_when_no_status_is_given()
    {
        var record = PlanningRecord.Create(
            OrganizationId, null, AssignedUserId, "Onderhoud", null, null,
            Now, Now.AddHours(1), null, Now);

        Assert.Equal(PlanningStatus.Confirmed, record.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_a_blank_title(string title) =>
        Assert.Throws<ArgumentException>(() => Create(title: title));

    [Fact]
    public void Create_rejects_an_end_that_is_not_after_the_start() =>
        Assert.Throws<ArgumentException>(() => Create(Now, Now));

    [Fact]
    public void Create_rejects_a_duration_below_the_minimum() =>
        Assert.Throws<ArgumentException>(() => Create(Now, Now.AddMinutes(14)));

    [Fact]
    public void Create_accepts_exactly_the_minimum_duration()
    {
        var record = Create(Now, Now.Add(PlanningRecord.MinimumDuration));

        Assert.Equal(PlanningRecord.MinimumDuration, record.EndUtc - record.StartUtc);
    }

    [Fact]
    public void Create_falls_back_to_the_default_colour()
    {
        Assert.Equal(PlanningRecord.DefaultColor, Create(color: null).Color);
        Assert.Equal(PlanningRecord.DefaultColor, Create(color: "   ").Color);
    }

    [Fact]
    public void Create_normalizes_a_colour_to_upper_case() =>
        Assert.Equal("#14B8A6", Create(color: " #14b8a6 ").Color);

    [Theory]
    [InlineData("6366F1")]
    [InlineData("#6366F")]
    [InlineData("#6366F12")]
    [InlineData("#GGGGGG")]
    public void Create_rejects_a_malformed_colour(string color) =>
        Assert.Throws<ArgumentException>(() => Create(color: color));

    [Fact]
    public void Update_applies_the_same_validation_as_create()
    {
        var record = Create();

        Assert.Throws<ArgumentException>(() =>
            record.Update(null, AssignedUserId, "", null, null, Now, Now.AddHours(1), PlanningRecord.DefaultColor, Now));
        Assert.Throws<ArgumentException>(() =>
            record.Update(null, AssignedUserId, "Titel", null, null, Now, Now.AddMinutes(5), PlanningRecord.DefaultColor, Now));
    }

    [Fact]
    public void Update_touches_the_timestamp()
    {
        var record = Create();
        var later = Now.AddMinutes(30);

        record.Update(null, AssignedUserId, "Nieuw", null, null, Now, Now.AddHours(2), "#14B8A6", later);

        Assert.Equal("Nieuw", record.Title);
        Assert.Equal(later, record.UpdatedAtUtc);
        Assert.Equal(Now, record.CreatedAtUtc);
    }

    [Fact]
    public void Move_keeps_the_title_and_validates_the_new_range()
    {
        var record = Create();

        record.Move(AssignedUserId, null, Now.AddDays(1), Now.AddDays(1).AddHours(1), Now);

        Assert.Equal("Onderhoud", record.Title);
        Assert.Equal(Now.AddDays(1), record.StartUtc);
        Assert.Throws<ArgumentException>(() =>
            record.Move(AssignedUserId, null, Now, Now.AddMinutes(1), Now));
    }

    [Fact]
    public void Duplicate_always_lands_as_planned_and_belongs_to_the_same_organization()
    {
        var original = Create(status: PlanningStatus.Completed);

        var copy = original.Duplicate(null, Now.AddDays(1), Now.AddDays(1).AddHours(1), Now);

        Assert.Equal(PlanningStatus.Planned, copy.Status);
        Assert.Equal(OrganizationId, copy.OrganizationId);
        Assert.Equal(original.AssignedUserId, copy.AssignedUserId);
        Assert.NotEqual(original.Id, copy.Id);
    }

    [Fact]
    public void Duplicate_validates_the_new_range()
    {
        var original = Create();

        Assert.Throws<ArgumentException>(() => original.Duplicate(null, Now, Now.AddMinutes(5), Now));
    }
}
