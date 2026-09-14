using FluentValidation;
using Planning.Application.Customers;
using Planning.Application.Organizations;
using Planning.Application.Planning;
using Planning.Domain.Enums;

namespace Planning.Tests.Application;

/// <summary>
/// The create and update requests used to have byte-identical but separate validators, so a rule
/// could be tightened on one path and silently not on the other. They now share a rule set;
/// these tests hold that shared behaviour in place.
/// </summary>
public class ValidatorParityTests
{
    private static readonly DateTime Start = new(2026, 8, 18, 9, 0, 0, DateTimeKind.Utc);

    private static IEnumerable<string> Failures<T>(IValidator<T> validator, T request) =>
        validator.Validate(request).Errors.Select(e => e.PropertyName).Distinct().OrderBy(x => x);

    [Fact]
    public void Both_planning_validators_reject_the_same_invalid_request()
    {
        var create = Failures(
            new CreatePlanningRequestValidator(),
            new CreatePlanningRequest(Guid.Empty, null, "", null, null, Start, Start, null));

        var update = Failures(
            new UpdatePlanningRequestValidator(),
            new UpdatePlanningRequest(Guid.Empty, null, "", null, null, Start, Start, PlanningStatus.Planned, null));

        Assert.NotEmpty(create);
        Assert.Equal(create, update);
    }

    [Fact]
    public void Both_customer_validators_reject_the_same_invalid_request()
    {
        var create = Failures(new CreateCustomerRequestValidator(), new CreateCustomerRequest("", "geen-email", null, "geen-kleur"));
        var update = Failures(new UpdateCustomerRequestValidator(), new UpdateCustomerRequest("", "geen-email", null, "geen-kleur"));

        Assert.NotEmpty(create);
        Assert.Equal(create, update);
    }

    [Fact]
    public void Both_organization_validators_reject_the_same_invalid_request()
    {
        var create = Failures(new CreateOrganizationRequestValidator(), new CreateOrganizationRequest("", "geen-email"));
        var update = Failures(new UpdateOrganizationRequestValidator(), new UpdateOrganizationRequest("", "geen-email"));

        Assert.NotEmpty(create);
        Assert.Equal(create, update);
    }

    [Fact]
    public void A_valid_planning_request_passes()
    {
        var result = new CreatePlanningRequestValidator().Validate(
            new CreatePlanningRequest(Guid.NewGuid(), null, "Onderhoud", null, null, Start, Start.AddHours(1), "#6366F1"));

        Assert.True(result.IsValid);
    }
}
