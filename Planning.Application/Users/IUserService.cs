using Planning.Application.Common;
using Planning.Application.Modules;

namespace Planning.Application.Users;

public interface IUserService
{
    Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<UserResponse>>> GetByOrganizationAsync(CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> UpdateRoleAsync(
        Guid id,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ModuleSettingResponse>>> UpdateModulesAsync(
        Guid id,
        UpdateModulesRequest request,
        CancellationToken cancellationToken = default);
}
