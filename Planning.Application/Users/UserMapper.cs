using Planning.Application.Modules;
using Planning.Domain.Users;

namespace Planning.Application.Users;

internal static class UserMapper
{
    public static UserResponse ToResponse(
        User user,
        IReadOnlyList<ModuleSettingResponse> modules) =>
        new(
            user.Id,
            user.OrganizationId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAtUtc,
            user.UpdatedAtUtc,
            modules);
}
