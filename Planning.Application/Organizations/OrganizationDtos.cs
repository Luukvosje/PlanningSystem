using Planning.Application.Modules;
using Planning.Domain.Enums;

namespace Planning.Application.Organizations;

/// <summary>Fields shared by the create and update requests, so one validator covers both.</summary>
public interface IOrganizationRequestFields
{
    string Name { get; }
    string Email { get; }
}

public sealed record CreateOrganizationRequest(string Name, string Email) : IOrganizationRequestFields;

public sealed record UpdateOrganizationRequest(string Name, string Email) : IOrganizationRequestFields;

public sealed record OpeningHoursEntryRequest(Weekday Day, string OpenTime, string CloseTime);

public sealed record ImportantWorkTimeRequest(string? Label, string StartTime);

public sealed record UpdateOrganizationPlanningSettingsRequest(
    IReadOnlyList<ImportantWorkTimeRequest> ImportantWorkTimes,
    IReadOnlyList<OpeningHoursEntryRequest> OpeningHours);

public sealed record OpeningHoursEntryResponse(Weekday Day, string OpenTime, string CloseTime);

public sealed record ImportantWorkTimeResponse(string? Label, string StartTime);

public sealed record OrganizationResponse(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<ModuleSettingResponse> Modules,
    IReadOnlyList<ImportantWorkTimeResponse> ImportantWorkTimes,
    IReadOnlyList<OpeningHoursEntryResponse> OpeningHours,
    string? LogoUrl);

public sealed record OrganizationLogoUploadResponse(string LogoUrl);