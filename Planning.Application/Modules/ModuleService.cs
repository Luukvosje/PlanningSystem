using Planning.Application.Common;
using Planning.Domain.Enums;
using Planning.Domain.Modules;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Application.Modules;

public class ModuleService : IModuleService
{
    private static readonly AppModule[] AllModules =
        [AppModule.Planning, AppModule.Klant, AppModule.Beheer];

    private readonly IModuleRepository _moduleRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public ModuleService(IModuleRepository moduleRepository, ICurrentUserContext currentUserContext)
    {
        _moduleRepository = moduleRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyList<ModuleSettingResponse>> GetOrganizationModulesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var modules = await _moduleRepository.GetOrganizationModulesAsync(organizationId, cancellationToken);
        return ToResponses(modules);
    }

    public async Task<IReadOnlyList<ModuleSettingResponse>> GetUserModulesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var modules = await _moduleRepository.GetUserModulesAsync(userId, cancellationToken);
        return ToResponses(modules);
    }

    public async Task<IReadOnlyList<ModuleSettingResponse>> GetEffectiveModulesAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var orgModules = await _moduleRepository.GetOrganizationModulesAsync(organizationId, cancellationToken);
        var userModules = await _moduleRepository.GetUserModulesAsync(userId, cancellationToken);

        var orgByKey = orgModules.ToDictionary(x => x.Module);
        var userByKey = userModules.ToDictionary(x => x.Module);

        return AllModules
            .Select(module =>
            {
                var orgEnabled = orgByKey.TryGetValue(module, out var orgModule) && orgModule.IsEnabled;
                var userEnabled = userByKey.TryGetValue(module, out var userModule) && userModule.IsEnabled;
                return new ModuleSettingResponse(module, orgEnabled && userEnabled);
            })
            .ToList();
    }

    public async Task InitializeOrganizationModulesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var modules = AllModules
            .Select(module => OrganizationModule.Create(organizationId, module, isEnabled: true))
            .ToList();

        await _moduleRepository.AddOrganizationModulesAsync(modules, cancellationToken);
    }

    public async Task InitializeUserModulesFromOrganizationAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var orgModules = await _moduleRepository.GetOrganizationModulesAsync(organizationId, cancellationToken);
        var orgByKey = orgModules.ToDictionary(x => x.Module);

        var modules = AllModules
            .Select(module =>
            {
                var isEnabled = orgByKey.TryGetValue(module, out var orgModule) && orgModule.IsEnabled;
                return UserModule.Create(userId, module, isEnabled);
            })
            .ToList();

        await _moduleRepository.AddUserModulesAsync(modules, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<ModuleSettingResponse>>> UpdateOrganizationModulesAsync(
        UpdateModulesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<IReadOnlyList<ModuleSettingResponse>>.Failure(
                "Organization context is required.",
                "NO_ORGANIZATION");
        }

        if (_currentUserContext.Role is not (UserRole.Owner or UserRole.Admin))
        {
            return Result<IReadOnlyList<ModuleSettingResponse>>.Failure(
                "Only owners and admins can update organization modules.",
                "FORBIDDEN");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;
        var updates = ToDictionary(request);

        await _moduleRepository.UpdateOrganizationModulesAsync(organizationId, updates, cancellationToken);

        var modules = await GetOrganizationModulesAsync(organizationId, cancellationToken);
        return Result<IReadOnlyList<ModuleSettingResponse>>.Success(modules);
    }

    public async Task<Result<IReadOnlyList<ModuleSettingResponse>>> UpdateUserModulesAsync(
        Guid userId,
        UpdateModulesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<IReadOnlyList<ModuleSettingResponse>>.Failure(
                "Organization context is required.",
                "NO_ORGANIZATION");
        }

        if (_currentUserContext.Role is not (UserRole.Owner or UserRole.Admin))
        {
            return Result<IReadOnlyList<ModuleSettingResponse>>.Failure(
                "Only owners and admins can update user modules.",
                "FORBIDDEN");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;
        var orgModules = await _moduleRepository.GetOrganizationModulesAsync(organizationId, cancellationToken);
        var orgByKey = orgModules.ToDictionary(x => x.Module);
        var updates = ToDictionary(request);

        foreach (var (module, isEnabled) in updates)
        {
            if (isEnabled
                && orgByKey.TryGetValue(module, out var orgModule)
                && !orgModule.IsEnabled)
            {
                return Result<IReadOnlyList<ModuleSettingResponse>>.Failure(
                    $"Module '{module}' is disabled at organization level.",
                    "VALIDATION_ERROR");
            }
        }

        await _moduleRepository.UpdateUserModulesAsync(userId, updates, cancellationToken);

        var modules = await GetUserModulesAsync(userId, cancellationToken);
        return Result<IReadOnlyList<ModuleSettingResponse>>.Success(modules);
    }

    public Task<bool> HasEffectiveModuleAsync(
        Guid userId,
        Guid organizationId,
        AppModule module,
        CancellationToken cancellationToken = default) =>
        _moduleRepository.HasEffectiveModuleAsync(userId, organizationId, module, cancellationToken);

    private static IReadOnlyDictionary<AppModule, bool> ToDictionary(UpdateModulesRequest request) =>
        new Dictionary<AppModule, bool>
        {
            [AppModule.Planning] = request.Planning,
            [AppModule.Klant] = request.Klant,
            [AppModule.Beheer] = request.Beheer,
        };

    private static IReadOnlyList<ModuleSettingResponse> ToResponses(IEnumerable<OrganizationModule> modules) =>
        modules.Select(x => new ModuleSettingResponse(x.Module, x.IsEnabled)).ToList();

    private static IReadOnlyList<ModuleSettingResponse> ToResponses(IEnumerable<UserModule> modules) =>
        modules.Select(x => new ModuleSettingResponse(x.Module, x.IsEnabled)).ToList();
}
