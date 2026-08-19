using Planning.Application.Availability;
using Planning.Application.Common;
using Planning.Application.Planning;
using Planning.Domain.Enums;

namespace Planning.Tests.Application;

/// <summary>
/// A typo in a query-string filter must produce a 400, not an unhandled exception.
/// </summary>
public class CsvListTests
{
    [Fact]
    public void Parses_a_comma_separated_guid_list_and_tolerates_spacing()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        Assert.True(CsvList.TryParseGuids($" {a} , {b} ", out var result));
        Assert.Equal([a, b], result);
    }

    [Fact]
    public void Treats_an_empty_filter_as_no_filter()
    {
        Assert.True(CsvList.TryParseGuids(null, out var fromNull));
        Assert.Null(fromNull);
        Assert.True(CsvList.TryParseGuids("   ", out var fromBlank));
        Assert.Null(fromBlank);
    }

    [Fact]
    public void Rejects_a_malformed_guid_list() =>
        Assert.False(CsvList.TryParseGuids($"{Guid.NewGuid()},not-a-guid", out _));

    [Fact]
    public void Parses_status_names_case_insensitively()
    {
        Assert.True(CsvList.TryParseEnums<PlanningStatus>("planned,CONFIRMED", out var result));
        Assert.Equal([PlanningStatus.Planned, PlanningStatus.Confirmed], result);
    }

    [Fact]
    public void Rejects_an_unknown_status_name() =>
        Assert.False(CsvList.TryParseEnums<PlanningStatus>("Planned,Vergeten", out _));

    [Fact]
    public void Rejects_a_numeric_status_outside_the_enum() =>
        Assert.False(CsvList.TryParseEnums<PlanningStatus>("99", out _));

    [Fact]
    public void The_planning_list_validator_rejects_a_malformed_filter()
    {
        var validator = new PlanningListRequestValidator();
        var request = new PlanningListRequest(
            new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 24, 0, 0, 0, DateTimeKind.Utc),
            UserIds: "not-a-guid",
            CustomerIds: null,
            Statuses: null,
            Search: null);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PlanningListRequest.UserIds));
    }

    [Fact]
    public void The_availability_validator_rejects_a_malformed_employee_filter()
    {
        var validator = new PlanningAvailabilityRequestValidator();
        var request = new PlanningAvailabilityRequest(
            new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 23), "1,2,3");

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PlanningAvailabilityRequest.EmployeeIds));
    }
}
