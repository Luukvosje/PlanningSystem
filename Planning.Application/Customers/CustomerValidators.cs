using FluentValidation;

namespace Planning.Application.Customers;

/// <summary>
/// One rule set for both the create and update request; they validate the same fields, and
/// keeping two copies meant a rule could be tightened on one path and not the other.
/// </summary>
public abstract class CustomerRequestValidator<T> : AbstractValidator<T>
    where T : ICustomerRequestFields
{
    protected CustomerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Address).MaximumLength(500);
    }
}

public class CreateCustomerRequestValidator : CustomerRequestValidator<CreateCustomerRequest>;

public class UpdateCustomerRequestValidator : CustomerRequestValidator<UpdateCustomerRequest>;
