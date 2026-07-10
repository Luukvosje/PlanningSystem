using FluentValidation;
using Planning.Domain.Enums;

namespace Planning.Application.Availability;

public class CreateAvailabilityRuleRequestValidator : AbstractValidator<CreateAvailabilityRuleRequest>
{
    public CreateAvailabilityRuleRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
        RuleFor(x => x.Reason).MaximumLength(500);

        RuleFor(x => x.Weekday)
            .NotNull()
            .When(x => x.Type == AvailabilityRuleType.Weekly);

        RuleFor(x => x.Date)
            .NotNull()
            .When(x => x.Type == AvailabilityRuleType.OneTime);

        RuleFor(x => x.Weekday)
            .Null()
            .When(x => x.Type == AvailabilityRuleType.OneTime);

        RuleFor(x => x.Date)
            .Null()
            .When(x => x.Type == AvailabilityRuleType.Weekly);

        RuleFor(x => x.Status)
            .Equal(AvailabilityRuleStatus.Unavailable)
            .WithMessage("Only unavailable rules are supported at this time.");
    }
}

public class UpdateAvailabilityRuleRequestValidator : AbstractValidator<UpdateAvailabilityRuleRequest>
{
    public UpdateAvailabilityRuleRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
        RuleFor(x => x.Reason).MaximumLength(500);

        RuleFor(x => x.Status)
            .Equal(AvailabilityRuleStatus.Unavailable)
            .WithMessage("Only unavailable rules are supported at this time.");
    }
}

public class PlanningAvailabilityRequestValidator : AbstractValidator<PlanningAvailabilityRequest>
{
    public PlanningAvailabilityRequestValidator()
    {
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
    }
}
