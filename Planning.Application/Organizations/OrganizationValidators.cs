using FluentValidation;

namespace Planning.Application.Organizations;

public class CreateOrganizationRequestValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
    }
}
