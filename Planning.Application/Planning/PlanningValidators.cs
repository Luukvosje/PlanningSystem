using FluentValidation;
using Planning.Application.Common;
using Planning.Domain.Enums;

namespace Planning.Application.Planning;

/// <summary>
/// One rule set for both the create and update request; they validate the same fields, and
/// keeping two copies meant a rule could be tightened on one path and not the other.
/// </summary>
public abstract class PlanningRequestValidator<T> : AbstractValidator<T>
    where T : IPlanningRequestFields
{
    protected PlanningRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Notes).MaximumLength(4000);
        RuleFor(x => x.Color).MaximumLength(7);
        RuleFor(x => x.EndUtc).GreaterThan(x => x.StartUtc);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class CreatePlanningRequestValidator : PlanningRequestValidator<CreatePlanningRequest>;

public class UpdatePlanningRequestValidator : PlanningRequestValidator<UpdatePlanningRequest>;

public class MovePlanningRequestValidator : AbstractValidator<MovePlanningRequest>
{
    public MovePlanningRequestValidator()
    {
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
        RuleFor(x => x.UserIds).MustBeGuidList();
        RuleFor(x => x.CustomerIds).MustBeGuidList();
        RuleFor(x => x.Statuses).MustBeEnumList<PlanningListRequest, PlanningStatus>();
    }
}
