using Planning.Application.Modules;
using Planning.Domain.Enums;

namespace Planning.Application.Users;

public sealed record UserResponse(
    Guid Id,
    Guid OrganizationId,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<ModuleSettingResponse> Modules);
public sealed record UpdateUserRoleRequest(UserRole Role);
