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
    string RefreshToken,
    bool RequiresOrganizationSelection,
    IReadOnlyList<OrganizationMembershipResponse>? Memberships,
    Guid? DefaultOrganizationId = null);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record OrganizationMembershipResponse(
    Guid OrganizationId,
    string OrganizationName,
    Guid UserId,
    UserRole Role,
    bool IsActive);

public sealed record CurrentUserResponse(
    Guid UserId,
    Guid AccountId,
    Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    string OrganizationName,
    bool RequiresApproval,
    IReadOnlyList<ModuleSettingResponse> Modules);

public sealed record UpdateProfileRequest(
    string Email,
    string FirstName,
    string LastName);

public sealed record UpdateProfileResponse(
    Guid AccountId,
    string Email,
    string FirstName,
    string LastName);


public sealed record TokenResponse(string AccessToken, string RefreshToken);
