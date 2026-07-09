using Planning.Application.Modules;
using Planning.Domain.Enums;

namespace Planning.Application.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);

public sealed record RegisterResponse(
    Guid AccountId,
    string Email);

public sealed record LoginRequest(
    string Email,
    string Password,
    Guid? OrganizationId);

public sealed record LoginResponse(
    string AccessToken,
    bool RequiresOrganizationSelection,
    IReadOnlyList<OrganizationMembershipResponse>? Memberships,
    Guid? DefaultOrganizationId = null);

public sealed record OrganizationMembershipResponse(
    Guid OrganizationId,
    string OrganizationName,
    Guid UserId,
    UserRole Role);

public sealed record CurrentUserResponse(
    Guid UserId,
    Guid AccountId,
    Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    string OrganizationName,
    IReadOnlyList<ModuleSettingResponse> Modules);

public sealed record SwitchOrganizationRequest(Guid OrganizationId);

public sealed record TokenResponse(string AccessToken);
