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
    private const long MaxLogoBytes = 2 * 1024 * 1024;

    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IModuleService _moduleService;
    private readonly IOrganizationLogoStorage _logoStorage;

    public OrganizationService(
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IAccountRepository accountRepository,
        ICurrentUserContext currentUserContext,
        IModuleService moduleService,
        IOrganizationLogoStorage logoStorage)
    {
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _currentUserContext = currentUserContext;
        _moduleService = moduleService;
        _logoStorage = logoStorage;
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
                new CreateOrganizationResponse(ToResponse(organization, modules)));
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
        return Result<OrganizationResponse>.Success(ToResponse(organization, modules));
    }

    public async Task<Result<OrganizationLogoFile>> GetCurrentLogoAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<OrganizationLogoFile>.Failure("No organization context.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;

        var membership = await _userRepository.GetByAccountAndOrganizationAsync(
            _currentUserContext.AccountId,
            organizationId,
            cancellationToken);

        if (membership is null)
        {
            return Result<OrganizationLogoFile>.Failure("Organization not found.", "NOT_FOUND");
        }

        var logo = _logoStorage.Get(organizationId);

        if (logo is null)
        {
            return Result<OrganizationLogoFile>.Failure("Logo not found.", "NOT_FOUND");
        }

        return Result<OrganizationLogoFile>.Success(logo);
    }

    public async Task<Result<OrganizationLogoUploadResponse>> UploadCurrentLogoAsync(
        Stream content,
        string contentType,
        long size,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<OrganizationLogoUploadResponse>.Failure("No organization context.", "NO_ORGANIZATION");
        }

        if (size <= 0)
        {
            return Result<OrganizationLogoUploadResponse>.Failure("No file uploaded.", "VALIDATION_ERROR");
        }

        if (size > MaxLogoBytes)
        {
            return Result<OrganizationLogoUploadResponse>.Failure(
                "Logo must be 2 MB or smaller.",
                "VALIDATION_ERROR");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;

        var membership = await _userRepository.GetByAccountAndOrganizationAsync(
            _currentUserContext.AccountId,
            organizationId,
            cancellationToken);

        if (membership is null)
        {
            return Result<OrganizationLogoUploadResponse>.Failure("Organization not found.", "NOT_FOUND");
        }

        try
        {
            await _logoStorage.SaveAsync(organizationId, content, contentType, cancellationToken);

            var logoUrl = _logoStorage.GetPublicUrl(organizationId);

            return Result<OrganizationLogoUploadResponse>.Success(
                new OrganizationLogoUploadResponse(logoUrl!));
        }
        catch (ArgumentException ex)
        {
            return Result<OrganizationLogoUploadResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    private OrganizationResponse ToResponse(
        Organization organization,
        IReadOnlyList<ModuleSettingResponse> modules) =>
        OrganizationMapper.ToResponse(organization, modules, _logoStorage.GetPublicUrl(organization.Id));

    public async Task<Result<OrganizationResponse>> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<OrganizationResponse>.Failure("No organization context.", "NO_ORGANIZATION");
        }

        return await GetByIdAsync(_currentUserContext.OrganizationId!.Value, cancellationToken);
    }

    public async Task<Result<OrganizationResponse>> UpdateCurrentAsync(
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<OrganizationResponse>.Failure("No organization context.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;

        var membership = await _userRepository.GetByAccountAndOrganizationAsync(
            _currentUserContext.AccountId,
            organizationId,
            cancellationToken);

        if (membership is null)
        {
            return Result<OrganizationResponse>.Failure("Organization not found.", "NOT_FOUND");
        }

        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return Result<OrganizationResponse>.Failure("Organization not found.", "NOT_FOUND");
        }

        if (await _organizationRepository.ExistsByEmailAsync(request.Email, organizationId, cancellationToken))
        {
            return Result<OrganizationResponse>.Failure("Email is already registered.", "CONFLICT");
        }

        try
        {
            var utcNow = DateTime.UtcNow;
            organization.Update(request.Name, request.Email, utcNow);
            await _organizationRepository.UpdateAsync(organization, cancellationToken);

            var modules = await _moduleService.GetOrganizationModulesAsync(organization.Id, cancellationToken);
            return Result<OrganizationResponse>.Success(ToResponse(organization, modules));
        }
        catch (ArgumentException ex)
        {
            return Result<OrganizationResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result<OrganizationResponse>> UpdateCurrentPlanningSettingsAsync(
        UpdateOrganizationPlanningSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<OrganizationResponse>.Failure("No organization context.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;

        var membership = await _userRepository.GetByAccountAndOrganizationAsync(
            _currentUserContext.AccountId,
            organizationId,
            cancellationToken);

        if (membership is null)
        {
            return Result<OrganizationResponse>.Failure("Organization not found.", "NOT_FOUND");
        }

        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return Result<OrganizationResponse>.Failure("Organization not found.", "NOT_FOUND");
        }

        try
        {
            var utcNow = DateTime.UtcNow;
            organization.UpdatePlanningSettings(
                OrganizationPlanningSettingsParser.ParseImportantWorkTimes(request.ImportantWorkTimes),
                OrganizationPlanningSettingsParser.ParseOpeningHours(request.OpeningHours),
                utcNow);

            await _organizationRepository.UpdateAsync(organization, cancellationToken);

            var modules = await _moduleService.GetOrganizationModulesAsync(organization.Id, cancellationToken);
            return Result<OrganizationResponse>.Success(ToResponse(organization, modules));
        }
        catch (ArgumentException ex)
        {
            return Result<OrganizationResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
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
