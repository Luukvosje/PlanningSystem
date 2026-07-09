using Microsoft.AspNetCore.Authorization;
using Planning.Domain.Modules;
namespace Planning.Api.Authorization;

public sealed class ModuleRequirement : IAuthorizationRequirement
{
    public ModuleRequirement(AppModule module)
    {
        Module = module;
    }

    public AppModule Module { get; }
}
