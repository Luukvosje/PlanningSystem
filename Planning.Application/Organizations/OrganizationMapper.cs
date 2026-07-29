using Planning.Application.Modules;
using Planning.Domain.Organizations;

namespace Planning.Application.Organizations;

internal static class OrganizationMapper
{
    public static OrganizationResponse ToResponse(
        Organization organization,
        IReadOnlyList<ModuleSettingResponse> modules,
        string? logoUrl = null) =>
        new(
            organization.Id,
            organization.Name,
            organization.Email,
            organization.CreatedAtUtc,
            organization.UpdatedAtUtc,
            modules,
            organization.ImportantWorkTimes
                .Select(time => time.ToString("HH\\:mm"))
                .ToList(),
            organization.OpeningHours
                .Select(entry => new OpeningHoursEntryResponse(
                    entry.Day,
                    entry.OpenTime.ToString("HH\\:mm"),
                    entry.CloseTime.ToString("HH\\:mm")))
                .ToList(),
            logoUrl);
}
