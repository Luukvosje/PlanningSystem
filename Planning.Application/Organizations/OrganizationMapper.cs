using Planning.Application.Auth;
using Planning.Application.Modules;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Application.Organizations;

internal static class OrganizationMapper
{
    /// <summary>
    /// Maps a user's memberships to responses, resolving organization names from a single
    /// pre-fetched lookup rather than one query per membership.
    /// </summary>
    public static IReadOnlyList<OrganizationMembershipResponse> ToMembershipResponses(
        IReadOnlyList<User> memberships,
        IReadOnlyDictionary<Guid, string> organizationNames) =>
        memberships
            .Select(membership => new OrganizationMembershipResponse(
                membership.OrganizationId,
                organizationNames.GetValueOrDefault(membership.OrganizationId, "Unknown"),
                membership.Id,
                membership.Role))
            .ToList();

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
                .Select(entry => new ImportantWorkTimeResponse(
                    entry.Label,
                    entry.StartTime.ToString("HH\\:mm")))
                .ToList(),
            organization.OpeningHours
                .Select(entry => new OpeningHoursEntryResponse(
                    entry.Day,
                    entry.OpenTime.ToString("HH\\:mm"),
                    entry.CloseTime.ToString("HH\\:mm")))
                .ToList(),
            logoUrl);
}
