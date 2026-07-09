using FluentValidation;
using Planning.Domain.Enums;

namespace Planning.Application.Availability;

public class WeekAvailabilityRequestValidator : AbstractValidator<WeekAvailabilityRequest>
{
    public WeekAvailabilityRequestValidator()
    {
        RuleFor(x => x.WeekStartUtc).NotEmpty();
    }
}

public class UpsertDayPartAvailabilityRequestValidator : AbstractValidator<UpsertDayPartAvailabilityRequest>
{
    public UpsertDayPartAvailabilityRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DayPart).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}

public class UpsertTimeBlockAvailabilityRequestValidator : AbstractValidator<UpsertTimeBlockAvailabilityRequest>
{
    public UpsertTimeBlockAvailabilityRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}
