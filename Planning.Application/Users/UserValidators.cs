using FluentValidation;
using Planning.Domain.Enums;

namespace Planning.Application.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(320)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role).IsInEnum().NotEqual(UserRole.Owner);
    }
}

/// <summary>
/// Nothing to check - a bool is either value. It exists because the controller resolves an
/// <see cref="IValidator{T}"/> for every request type, and the real guards (owner, self) are
/// authorization decisions that need the loaded user, so they live in the service.
/// </summary>
public class UpdateUserStatusRequestValidator : AbstractValidator<UpdateUserStatusRequest>
{
}

/// <summary>
/// Same as the status validator: the request is a single bool, and whether the caller may set it
/// is an authorization decision that needs the loaded user, so it lives in the service.
/// </summary>
public class UpdateUserApprovalRequestValidator : AbstractValidator<UpdateUserApprovalRequest>
{
}
