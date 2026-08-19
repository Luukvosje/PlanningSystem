using Planning.Application.Common;
using Planning.Domain.Enums;
using Planning.Domain.Modules;

namespace Planning.Application.Modules;

public interface IModuleService
{
    Task<IReadOnlyList<ModuleSettingResponse>> GetOrganizationModulesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModuleSettingResponse>> GetUserModulesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, IReadOnlyList<ModuleSettingResponse>>> GetUserModulesByUsersAsync(
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModuleSettingResponse>> GetEffectiveModulesAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task InitializeOrganizationModulesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task InitializeUserModulesFromOrganizationAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ModuleSettingResponse>>> UpdateOrganizationModulesAsync(
        UpdateModulesRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ModuleSettingResponse>>> UpdateUserModulesAsync(
        Guid userId,
        UpdateModulesRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> HasEffectiveModuleAsync(
        Guid userId,
        Guid organizationId,
        AppModule module,
        CancellationToken cancellationToken = default);
}
