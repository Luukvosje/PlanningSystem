using Planning.Domain.Enums;
using Planning.Domain.Modules;

namespace Planning.Application.Modules;

public static class ModulePermissions
{
    public static bool IsAdminRole(UserRole role) =>
        role is UserRole.Owner or UserRole.Admin;

    public static bool GetOrganizationEffectiveEnabled(AppModule module, bool orgEnabled) =>
        module is AppModule.Beheer || orgEnabled;

    public static bool HasEffectiveAccess(UserRole role, bool orgEnabled, bool userEnabled) =>
        orgEnabled && (IsAdminRole(role) || userEnabled);
}
