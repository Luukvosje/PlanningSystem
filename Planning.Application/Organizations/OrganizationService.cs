using Planning.Application.Auth;
using Planning.Application.Common;
using Planning.Application.Modules;
using Planning.Domain.Auth;
using Planning.Domain.Enums;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Application.Organizations;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IModuleService _moduleService;

    public OrganizationService(
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IAccountRepository accountRepository,
        ICurrentUserContext currentUserContext,
        IModuleService moduleService)
    {
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _currentUserContext = currentUserContext;
        _moduleService = moduleService;
    }

    public async Task<Result<CreateOrganizationResponse>> CreateForAccountAsync(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.IsAuthenticated)
        {
            return Result<CreateOrganizationResponse>.Failure("Not authenticated.", "UNAUTHORIZED");
        }

        var account = await _accountRepository.GetByIdAsync(_currentUserContext.AccountId, cancellationToken);

        if (account is null)
        {
            return Result<CreateOrganizationResponse>.Failure("Account not found.", "NOT_FOUND");
        }

        try
        {
            var utcNow = DateTime.UtcNow;
            var organization = Organization.Create(request.Name, request.Email, utcNow);
            await _organizationRepository.AddAsync(organization, cancellationToken);

            var firstName = account.FirstName;
            var lastName = account.LastName;

            var user = User.Create(
                _currentUserContext.AccountId,
                organization.Id,
                firstName,
                lastName,
                account.Email,
                UserRole.Owner,
                utcNow);

            await _userRepository.AddAsync(user, cancellationToken);

            await _moduleService.InitializeOrganizationModulesAsync(organization.Id, cancellationToken);
            await _moduleService.InitializeUserModulesFromOrganizationAsync(user.Id, organization.Id, cancellationToken);

            var modules = await _moduleService.GetOrganizationModulesAsync(organization.Id, cancellationToken);

            return Result<CreateOrganizationResponse>.Success(
                new CreateOrganizationResponse(OrganizationMapper.ToResponse(organization, modules)));
        }
        catch (ArgumentException ex)
        {
            return Result<CreateOrganizationResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result<OrganizationResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.IsAuthenticated)
        {
            return Result<OrganizationResponse>.Failure("Not authenticated.", "UNAUTHORIZED");
        }

        var membership = await _userRepository.GetByAccountAndOrganizationAsync(
            _currentUserContext.AccountId,
            id,
            cancellationToken);

        if (membership is null)
        {
            return Result<OrganizationResponse>.Failure("Organization not found.", "NOT_FOUND");
        }

        var organization = await _organizationRepository.GetByIdAsync(id, cancellationToken);

        if (organization is null)
        {
            return Result<OrganizationResponse>.Failure("Organization not found.", "NOT_FOUND");
        }

        var modules = await _moduleService.GetOrganizationModulesAsync(organization.Id, cancellationToken);
        return Result<OrganizationResponse>.Success(OrganizationMapper.ToResponse(organization, modules));
    }

    public async Task<Result<OrganizationResponse>> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<OrganizationResponse>.Failure("No organization context.", "NO_ORGANIZATION");
        }

        return await GetByIdAsync(_currentUserContext.OrganizationId!.Value, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<OrganizationMembershipResponse>>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.IsAuthenticated)
        {
            return Result<IReadOnlyList<OrganizationMembershipResponse>>.Failure(
                "Not authenticated.",
                "UNAUTHORIZED");
        }

        var memberships = await _userRepository.GetByAccountIdAsync(
            _currentUserContext.AccountId,
            cancellationToken);

        var responses = new List<OrganizationMembershipResponse>();

        foreach (var membership in memberships)
        {
            var organization = await _organizationRepository.GetByIdAsync(
                membership.OrganizationId,
                cancellationToken);

            responses.Add(new OrganizationMembershipResponse(
                membership.OrganizationId,
                organization?.Name ?? "Unknown",
                membership.Id,
                membership.Role));
        }

        return Result<IReadOnlyList<OrganizationMembershipResponse>>.Success(responses);
    }
}
