using FluentValidation;

namespace Planning.Application.Invites;

public class CreateInviteRequestValidator : AbstractValidator<CreateInviteRequest>
{
    public CreateInviteRequestValidator()
    {
    }
}

public class AcceptInviteRequestValidator : AbstractValidator<AcceptInviteRequest>
{
    public AcceptInviteRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(16);
    }
}
