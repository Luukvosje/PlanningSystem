using Planning.Application.Common;
using Planning.Application.Modules;
using Planning.Domain.Enums;
using Planning.Domain.Users;

namespace Planning.Application.Users;

public class UserService : TenantServiceBase, IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IModuleService _moduleService;

    public UserService(
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext,
        IModuleService moduleService)
        : base(currentUserContext)
    {
        _userRepository = userRepository;
        _moduleService = moduleService;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.HasOrganization)
        {
            return Failures.NoOrganizationContext<UserResponse>();
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null || !Owns(user))
        {
            return Failures.NotFoundFor<UserResponse>("User");
        }

        var modules = await _moduleService.GetUserModulesAsync(user.Id, cancellationToken);
        return Result<UserResponse>.Success(UserMapper.ToResponse(user, modules));
    }

    public async Task<Result<IReadOnlyList<UserResponse>>> GetByOrganizationAsync(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<IReadOnlyList<UserResponse>>();
        }

        var users = await _userRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);

        var modulesByUser = await _moduleService.GetUserModulesByUsersAsync(
            users.Select(x => x.Id).ToList(),
            cancellationToken);

        var response = users
            .Select(user => UserMapper.ToResponse(
                user,
                modulesByUser.GetValueOrDefault(user.Id, [])))
            .ToList();

        return Result<IReadOnlyList<UserResponse>>.Success(response);
    }

    public async Task<Result<UserResponse>> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<UserResponse>();
        }

        if (CurrentUser.Role is not (UserRole.Owner or UserRole.Admin))
        {
            return Failures.ForbiddenFor<UserResponse>("Only owners and admins can add team members.");
        }

        // The same rule the unique index enforces, answered as a validation error instead of a 500.
        if (!string.IsNullOrWhiteSpace(request.Email)
            && await _userRepository.ExistsWithEmailAsync(organizationId, request.Email, cancellationToken))
        {
            return Result<UserResponse>.Failure(
                "A team member with this email already exists.",
                "CONFLICT");
        }

        return await TranslateDomainErrorsAsync(async () =>
        {
            var user = User.CreateWithoutAccount(
                organizationId,
                request.FirstName,
                request.LastName,
                request.Email,
                UserRole.Employee,
                DateTime.UtcNow);

            await _userRepository.AddAsync(user, cancellationToken);
            await _moduleService.InitializeUserModulesFromOrganizationAsync(user.Id, organizationId, cancellationToken);

            var modules = await _moduleService.GetUserModulesAsync(user.Id, cancellationToken);
            return Result<UserResponse>.Success(UserMapper.ToResponse(user, modules));
        });
    }

    public async Task<Result<UserResponse>> UpdateRoleAsync(
        Guid id,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.HasOrganization || CurrentUser.UserId is null)
        {
            return Failures.NoOrganizationContext<UserResponse>();
        }

        if (CurrentUser.Role is not (UserRole.Owner or UserRole.Admin))
        {
            return Failures.ForbiddenFor<UserResponse>("Only owners and admins can change roles.");
        }

        if (request.Role is UserRole.Owner)
        {
            return Result<UserResponse>.Failure("Cannot assign the owner role.", Failures.Validation);
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null || !Owns(user))
        {
            return Failures.NotFoundFor<UserResponse>("User");
        }

        if (user.Role is UserRole.Owner)
        {
            return Result<UserResponse>.Failure("Cannot change the owner role.", Failures.Validation);
        }

        if (user.Id == CurrentUser.UserId
            && CurrentUser.Role is UserRole.Admin
            && request.Role is not UserRole.Admin)
        {
            return Result<UserResponse>.Failure(
                "You cannot remove your own admin role.",
                Failures.Validation);
        }

        user.ChangeRole(request.Role, DateTime.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);

        var modules = await _moduleService.GetUserModulesAsync(user.Id, cancellationToken);
        return Result<UserResponse>.Success(UserMapper.ToResponse(user, modules));
    }

    public async Task<Result<UserResponse>> UpdateStatusAsync(
        Guid id,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.HasOrganization || CurrentUser.UserId is null)
        {
            return Failures.NoOrganizationContext<UserResponse>();
        }

        if (CurrentUser.Role is not (UserRole.Owner or UserRole.Admin))
        {
            return Failures.ForbiddenFor<UserResponse>(
                "Only owners and admins can activate or deactivate a team member.");
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null || !Owns(user))
        {
            return Failures.NotFoundFor<UserResponse>("User");
        }

        // Deactivating drops the member out of the organization: OrganizationContextMiddleware
        // never sets the context for an inactive user, so every tenant endpoint refuses them.
        // The account itself can still sign in - it may belong to another organization - but
        // neither guard below is recoverable from the UI, so both are blocked outright.
        if (!request.IsActive && user.Role is UserRole.Owner)
        {
            return Result<UserResponse>.Failure(
                "Cannot deactivate the owner.",
                Failures.Validation);
        }

        if (!request.IsActive && user.Id == CurrentUser.UserId)
        {
            return Result<UserResponse>.Failure(
                "You cannot deactivate yourself.",
                Failures.Validation);
        }

        if (user.IsActive != request.IsActive)
        {
            var utcNow = DateTime.UtcNow;

            if (request.IsActive)
            {
                user.Activate(utcNow);
            }
            else
            {
                user.Deactivate(utcNow);
            }

            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        var userModules = await _moduleService.GetUserModulesAsync(user.Id, cancellationToken);
        return Result<UserResponse>.Success(UserMapper.ToResponse(user, userModules));
    }

    public async Task<Result<UserResponse>> UpdateApprovalAsync(
        Guid id,
        UpdateUserApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.HasOrganization || CurrentUser.UserId is null)
        {
            return Failures.NoOrganizationContext<UserResponse>();
        }

        if (CurrentUser.Role is not (UserRole.Owner or UserRole.Admin))
        {
            return Failures.ForbiddenFor<UserResponse>(
                "Only owners and admins can change who has to request their availability.");
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null || !Owns(user))
        {
            return Failures.NotFoundFor<UserResponse>("User");
        }

        return await TranslateDomainErrorsAsync(async () =>
        {
            if (user.RequiresApproval != request.RequiresApproval)
            {
                user.SetRequiresApproval(request.RequiresApproval, DateTime.UtcNow);
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            var modules = await _moduleService.GetUserModulesAsync(user.Id, cancellationToken);
            return Result<UserResponse>.Success(UserMapper.ToResponse(user, modules));
        });
    }

    public async Task<Result<IReadOnlyList<ModuleSettingResponse>>> UpdateModulesAsync(
        Guid id,
        UpdateModulesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.HasOrganization)
        {
            return Failures.NoOrganizationContext<IReadOnlyList<ModuleSettingResponse>>();
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null || !Owns(user))
        {
            return Failures.NotFoundFor<IReadOnlyList<ModuleSettingResponse>>("User");
        }

        return await _moduleService.UpdateUserModulesAsync(id, request, cancellationToken);
    }
}
