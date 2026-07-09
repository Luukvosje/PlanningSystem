using Microsoft.AspNetCore.Authorization;
using Planning.Application.Common;
using Planning.Application.Modules;
using Planning.Domain.Enums;

namespace Planning.Api.Authorization;

public class ModuleAuthorizationHandler : AuthorizationHandler<ModuleRequirement>
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IModuleService _moduleService;

    public ModuleAuthorizationHandler(
        ICurrentUserContext currentUserContext,
        IModuleService moduleService)
    {
        _currentUserContext = currentUserContext;
        _moduleService = moduleService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModuleRequirement requirement)
    {
        if (!_currentUserContext.HasOrganization
            || _currentUserContext.UserId is null
            || _currentUserContext.OrganizationId is null)
        {
            return;
        }

        var hasModule = await _moduleService.HasEffectiveModuleAsync(
            _currentUserContext.UserId.Value,
            _currentUserContext.OrganizationId.Value,
            requirement.Module);

        if (hasModule)
        {
            context.Succeed(requirement);
        }
    }
}
