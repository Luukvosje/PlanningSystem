using Planning.Application.Modules;
using Planning.Domain.Enums;

namespace Planning.Application.Users;

public sealed record UserResponse(
    Guid Id,
    Guid OrganizationId,
    string FirstName,
    string LastName,
    string? Email,
    UserRole Role,
    bool IsActive,
    bool HasAccount,
    bool RequiresApproval,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<ModuleSettingResponse> Modules);
/// <summary>
/// A member added by a planner before the person has a login. Always an employee: the role is
/// changed through the role endpoint like for any other member.
/// </summary>
public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string? Email = null);

public sealed record UpdateUserRoleRequest(UserRole Role);
public sealed record UpdateUserStatusRequest(bool IsActive);
public sealed record UpdateUserApprovalRequest(bool RequiresApproval);
