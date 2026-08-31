using FluentValidation;

namespace Planning.Application.Requests;

public class DecideRequestsRequestValidator : AbstractValidator<DecideRequestsRequest>
{
    /// <summary>
    /// One call decides one row or a whole inbox ("approve all"), so the list is capped rather than
    /// unbounded: each item is its own load and save.
    /// </summary>
    private const int MaxRequestsPerCall = 200;

    public DecideRequestsRequestValidator()
    {
        RuleFor(x => x.Requests)
            .NotEmpty()
            .Must(requests => requests is null || requests.Count <= MaxRequestsPerCall)
            .WithMessage($"Cannot decide on more than {MaxRequestsPerCall} requests at once.");

        RuleForEach(x => x.Requests).ChildRules(reference =>
        {
            reference.RuleFor(x => x.Id).NotEmpty();
            reference.RuleFor(x => x.Kind).IsInEnum();
        });
    }
}
