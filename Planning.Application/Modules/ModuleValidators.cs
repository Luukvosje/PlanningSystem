using FluentValidation;

namespace Planning.Application.Modules;

public class UpdateModulesRequestValidator : AbstractValidator<UpdateModulesRequest>
{
    public UpdateModulesRequestValidator()
    {
    }
}
