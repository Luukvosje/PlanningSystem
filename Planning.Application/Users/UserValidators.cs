using FluentValidation;
using Planning.Domain.Enums;

namespace Planning.Application.Users;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role).IsInEnum().NotEqual(UserRole.Owner);
    }
}
