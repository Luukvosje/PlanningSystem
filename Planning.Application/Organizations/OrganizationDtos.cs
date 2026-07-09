using Planning.Application.Modules;

namespace Planning.Application.Organizations;

public sealed record CreateOrganizationRequest(string Name, string Email);

public sealed record OrganizationResponse(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<ModuleSettingResponse> Modules);