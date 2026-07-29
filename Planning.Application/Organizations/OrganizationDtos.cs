using Planning.Application.Modules;
using Planning.Domain.Enums;

namespace Planning.Application.Organizations;

public sealed record CreateOrganizationRequest(string Name, string Email);

public sealed record UpdateOrganizationRequest(string Name, string Email);

public sealed record OpeningHoursEntryRequest(Weekday Day, string OpenTime, string CloseTime);

public sealed record UpdateOrganizationPlanningSettingsRequest(
    IReadOnlyList<string> ImportantWorkTimes,
    IReadOnlyList<OpeningHoursEntryRequest> OpeningHours);

public sealed record OpeningHoursEntryResponse(Weekday Day, string OpenTime, string CloseTime);

public sealed record OrganizationResponse(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<ModuleSettingResponse> Modules,
    IReadOnlyList<string> ImportantWorkTimes,
    IReadOnlyList<OpeningHoursEntryResponse> OpeningHours,
    string? LogoUrl);

public sealed record OrganizationLogoUploadResponse(string LogoUrl);