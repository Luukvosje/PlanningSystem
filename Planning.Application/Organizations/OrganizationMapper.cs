using Planning.Application.Modules;
using Planning.Domain.Organizations;

namespace Planning.Application.Organizations;

internal static class OrganizationMapper
{
    public static OrganizationResponse ToResponse(
        Organization organization,
        IReadOnlyList<ModuleSettingResponse> modules) =>
        new(
            organization.Id,
            organization.Name,
            organization.Email,
            organization.CreatedAtUtc,
            organization.UpdatedAtUtc,
            modules);
}
