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
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public ModuleService(
        IModuleRepository moduleRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
    {
        _moduleRepository = moduleRepository;
        _userRepository = userRepository;
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
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return [];
        }

        var orgModules = await _moduleRepository.GetOrganizationModulesAsync(organizationId, cancellationToken);
        var userModules = await _moduleRepository.GetUserModulesAsync(userId, cancellationToken);

        return ToEffectiveResponses(user.Role, orgModules, userModules);
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
        var updates = new Dictionary<AppModule, bool>(ToDictionary(request))
        {
            [AppModule.Beheer] = true,
        };

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
        var targetUser = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (targetUser is null || targetUser.OrganizationId != organizationId)
        {
            return Result<IReadOnlyList<ModuleSettingResponse>>.Failure(
                "User not found.",
                "NOT_FOUND");
        }

        var orgModules = await _moduleRepository.GetOrganizationModulesAsync(organizationId, cancellationToken);
        var orgByKey = orgModules.ToDictionary(x => x.Module);
        var updates = new Dictionary<AppModule, bool>(ToDictionary(request));

        foreach (var module in AllModules)
        {
            if (!orgByKey.TryGetValue(module, out var orgModule))
            {
                continue;
            }

            var orgEnabled = ModulePermissions.GetOrganizationEffectiveEnabled(module, orgModule.IsEnabled);

            if (!orgEnabled)
            {
                if (updates.TryGetValue(module, out var isEnabled) && isEnabled)
                {
                    return Result<IReadOnlyList<ModuleSettingResponse>>.Failure(
                        $"Module '{module}' is disabled at organization level.",
                        "VALIDATION_ERROR");
                }

                updates[module] = false;
                continue;
            }

            if (ModulePermissions.IsAdminRole(targetUser.Role))
            {
                updates[module] = true;
            }
        }

        await _moduleRepository.UpdateUserModulesAsync(userId, updates, cancellationToken);

        var modules = await GetUserModulesAsync(userId, cancellationToken);
        return Result<IReadOnlyList<ModuleSettingResponse>>.Success(modules);
    }

    public async Task<bool> HasEffectiveModuleAsync(
        Guid userId,
        Guid organizationId,
        AppModule module,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return false;
        }

        var orgModules = await _moduleRepository.GetOrganizationModulesAsync(organizationId, cancellationToken);
        var userModules = await _moduleRepository.GetUserModulesAsync(userId, cancellationToken);

        var orgByKey = orgModules.ToDictionary(x => x.Module);
        var userByKey = userModules.ToDictionary(x => x.Module);

        var orgDbEnabled = orgByKey.TryGetValue(module, out var orgModule) && orgModule.IsEnabled;
        var orgEnabled = ModulePermissions.GetOrganizationEffectiveEnabled(module, orgDbEnabled);
        var userEnabled = userByKey.TryGetValue(module, out var userModule) && userModule.IsEnabled;

        return ModulePermissions.HasEffectiveAccess(user.Role, orgEnabled, userEnabled);
    }

    private static IReadOnlyList<ModuleSettingResponse> ToEffectiveResponses(
        UserRole role,
        IEnumerable<OrganizationModule> orgModules,
        IEnumerable<UserModule> userModules)
    {
        var orgByKey = orgModules.ToDictionary(x => x.Module);
        var userByKey = userModules.ToDictionary(x => x.Module);

        return AllModules
            .Select(module =>
            {
                var orgDbEnabled = orgByKey.TryGetValue(module, out var orgModule) && orgModule.IsEnabled;
                var orgEnabled = ModulePermissions.GetOrganizationEffectiveEnabled(module, orgDbEnabled);
                var userEnabled = userByKey.TryGetValue(module, out var userModule) && userModule.IsEnabled;
                return new ModuleSettingResponse(
                    module,
                    ModulePermissions.HasEffectiveAccess(role, orgEnabled, userEnabled));
            })
            .ToList();
    }

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
