using FluentValidation;

namespace Planning.Application.Planning;

public class CreatePlanningRequestValidator : AbstractValidator<CreatePlanningRequest>
{
    public CreatePlanningRequestValidator()
    {
        RuleFor(x => x.AssignedUserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Notes).MaximumLength(4000);
        RuleFor(x => x.Color).MaximumLength(7);
        RuleFor(x => x.EndUtc).GreaterThan(x => x.StartUtc);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class UpdatePlanningRequestValidator : AbstractValidator<UpdatePlanningRequest>
{
    public UpdatePlanningRequestValidator()
    {
        RuleFor(x => x.AssignedUserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Notes).MaximumLength(4000);
        RuleFor(x => x.Color).MaximumLength(7);
        RuleFor(x => x.EndUtc).GreaterThan(x => x.StartUtc);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class MovePlanningRequestValidator : AbstractValidator<MovePlanningRequest>
{
    public MovePlanningRequestValidator()
    {
        RuleFor(x => x.AssignedUserId).NotEmpty();
        RuleFor(x => x.EndUtc).GreaterThan(x => x.StartUtc);
    }
}

public class DuplicatePlanningRequestValidator : AbstractValidator<DuplicatePlanningRequest>
{
    public DuplicatePlanningRequestValidator()
    {
    }
}

public class PlanningListRequestValidator : AbstractValidator<PlanningListRequest>
{
    public PlanningListRequestValidator()
    {
        RuleFor(x => x.StartUtc).NotEmpty();
        RuleFor(x => x.EndUtc).GreaterThan(x => x.StartUtc);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 2000);
    }
}

public class WeekPlanningRequestValidator : AbstractValidator<WeekPlanningRequest>
{
    public WeekPlanningRequestValidator()
    {
        RuleFor(x => x.WeekStartUtc).NotEmpty();
    }
}
