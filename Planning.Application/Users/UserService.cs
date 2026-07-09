using Planning.Application.Common;
using Planning.Application.Modules;
using Planning.Domain.Enums;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Application.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IModuleService _moduleService;

    public UserService(
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext,
        IModuleService moduleService)
    {
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
        _moduleService = moduleService;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<UserResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null || user.OrganizationId != _currentUserContext.OrganizationId)
        {
            return Result<UserResponse>.Failure("User not found.", "NOT_FOUND");
        }

        var modules = await _moduleService.GetUserModulesAsync(user.Id, cancellationToken);
        return Result<UserResponse>.Success(UserMapper.ToResponse(user, modules));
    }

    public async Task<Result<IReadOnlyList<UserResponse>>> GetByOrganizationAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<IReadOnlyList<UserResponse>>.Failure(
                "Organization context is required.",
                "NO_ORGANIZATION");
        }

        var users = await _userRepository.GetByOrganizationIdAsync(
            _currentUserContext.OrganizationId!.Value,
            cancellationToken);

        var response = new List<UserResponse>();
        foreach (var user in users)
        {
            var modules = await _moduleService.GetUserModulesAsync(user.Id, cancellationToken);
            response.Add(UserMapper.ToResponse(user, modules));
        }

        return Result<IReadOnlyList<UserResponse>>.Success(response);
    }

    public async Task<Result<UserResponse>> UpdateRoleAsync(
        Guid id,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization || _currentUserContext.UserId is null)
        {
            return Result<UserResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        if (_currentUserContext.Role is not (UserRole.Owner or UserRole.Admin))
        {
            return Result<UserResponse>.Failure("Only owners and admins can change roles.", "FORBIDDEN");
        }

        if (request.Role is UserRole.Owner)
        {
            return Result<UserResponse>.Failure("Cannot assign the owner role.", "VALIDATION_ERROR");
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null || user.OrganizationId != _currentUserContext.OrganizationId)
        {
            return Result<UserResponse>.Failure("User not found.", "NOT_FOUND");
        }

        if (user.Role is UserRole.Owner)
        {
            return Result<UserResponse>.Failure("Cannot change the owner role.", "VALIDATION_ERROR");
        }

        if (user.Id == _currentUserContext.UserId
            && _currentUserContext.Role is UserRole.Admin
            && request.Role is not UserRole.Admin)
        {
            return Result<UserResponse>.Failure(
                "You cannot remove your own admin role.",
                "VALIDATION_ERROR");
        }

        user.ChangeRole(request.Role, DateTime.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);

        var modules = await _moduleService.GetUserModulesAsync(user.Id, cancellationToken);
        return Result<UserResponse>.Success(UserMapper.ToResponse(user, modules));
    }

    public async Task<Result<IReadOnlyList<ModuleSettingResponse>>> UpdateModulesAsync(
        Guid id,
        UpdateModulesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<IReadOnlyList<ModuleSettingResponse>>.Failure(
                "Organization context is required.",
                "NO_ORGANIZATION");
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null || user.OrganizationId != _currentUserContext.OrganizationId)
        {
            return Result<IReadOnlyList<ModuleSettingResponse>>.Failure("User not found.", "NOT_FOUND");
        }

        return await _moduleService.UpdateUserModulesAsync(id, request, cancellationToken);
    }
}
