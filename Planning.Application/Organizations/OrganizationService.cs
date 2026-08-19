using Planning.Application.Auth;
using Planning.Application.Common;
using Planning.Application.Modules;
using Planning.Domain.Auth;
using Planning.Domain.Enums;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Application.Organizations;

public class OrganizationService : TenantServiceBase, IOrganizationService
{
    private const long MaxLogoBytes = 2 * 1024 * 1024;

    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IModuleService _moduleService;
    private readonly IOrganizationLogoStorage _logoStorage;

    public OrganizationService(
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IAccountRepository accountRepository,
        ICurrentUserContext currentUserContext,
        IModuleService moduleService,
        IOrganizationLogoStorage logoStorage)
        : base(currentUserContext)
    {
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _moduleService = moduleService;
        _logoStorage = logoStorage;
    }

    public async Task<Result<CreateOrganizationResponse>> CreateForAccountAsync(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return Result<CreateOrganizationResponse>.Failure("Not authenticated.", "UNAUTHORIZED");
        }

        var account = await _accountRepository.GetByIdAsync(CurrentUser.AccountId, cancellationToken);

        if (account is null)
        {
            return Result<CreateOrganizationResponse>.Failure("Account not found.", Failures.NotFound);
        }

        try
        {
            var utcNow = DateTime.UtcNow;
            var organization = Organization.Create(request.Name, request.Email, utcNow);
            await _organizationRepository.AddAsync(organization, cancellationToken);

            var firstName = account.FirstName;
            var lastName = account.LastName;

            var user = User.Create(
                CurrentUser.AccountId,
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
            return Result<CreateOrganizationResponse>.Failure(ex.Message, Failures.Validation);
        }
    }

    public async Task<Result<OrganizationResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return Result<OrganizationResponse>.Failure("Not authenticated.", "UNAUTHORIZED");
        }

        if (!await IsMemberOfAsync(id, cancellationToken))
        {
            return Failures.NotFoundFor<OrganizationResponse>("Organization");
        }

        var organization = await _organizationRepository.GetByIdAsync(id, cancellationToken);

        if (organization is null)
        {
            return Failures.NotFoundFor<OrganizationResponse>("Organization");
        }

        var modules = await _moduleService.GetOrganizationModulesAsync(organization.Id, cancellationToken);
        return Result<OrganizationResponse>.Success(ToResponse(organization, modules));
    }

    public async Task<Result<OrganizationLogoFile>> GetCurrentLogoAsync(
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.HasOrganization)
        {
            return Failures.NoOrganizationContext<OrganizationLogoFile>();
        }

        var organizationId = CurrentUser.OrganizationId!.Value;

        if (!await IsMemberOfAsync(organizationId, cancellationToken))
        {
            return Failures.NotFoundFor<OrganizationLogoFile>("Organization");
        }

        var logo = _logoStorage.Get(organizationId);

        if (logo is null)
        {
            return Result<OrganizationLogoFile>.Failure("Logo not found.", Failures.NotFound);
        }

        return Result<OrganizationLogoFile>.Success(logo);
    }

    public async Task<Result<OrganizationLogoUploadResponse>> UploadCurrentLogoAsync(
        Stream content,
        string contentType,
        long size,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.HasOrganization)
        {
            return Failures.NoOrganizationContext<OrganizationLogoUploadResponse>();
        }

        if (size <= 0)
        {
            return Result<OrganizationLogoUploadResponse>.Failure("No file uploaded.", Failures.Validation);
        }

        if (size > MaxLogoBytes)
        {
            return Result<OrganizationLogoUploadResponse>.Failure(
                "Logo must be 2 MB or smaller.",
                Failures.Validation);
        }

        var organizationId = CurrentUser.OrganizationId!.Value;

        if (!await IsMemberOfAsync(organizationId, cancellationToken))
        {
            return Failures.NotFoundFor<OrganizationLogoUploadResponse>("Organization");
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
            return Result<OrganizationLogoUploadResponse>.Failure(ex.Message, Failures.Validation);
        }
    }

    /// <summary>
    /// Whether the signed-in account is a member of this organization. A non-member gets
    /// NOT_FOUND rather than FORBIDDEN: they have no business learning the organization exists.
    /// </summary>
    private async Task<bool> IsMemberOfAsync(Guid organizationId, CancellationToken cancellationToken) =>
        await _userRepository.GetByAccountAndOrganizationAsync(
            CurrentUser.AccountId,
            organizationId,
            cancellationToken) is not null;

    private OrganizationResponse ToResponse(
        Organization organization,
        IReadOnlyList<ModuleSettingResponse> modules) =>
        OrganizationMapper.ToResponse(organization, modules, _logoStorage.GetPublicUrl(organization.Id));

    public async Task<Result<OrganizationResponse>> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.HasOrganization)
        {
            return Failures.NoOrganizationContext<OrganizationResponse>();
        }

        return await GetByIdAsync(CurrentUser.OrganizationId!.Value, cancellationToken);
    }

    public async Task<Result<OrganizationResponse>> UpdateCurrentAsync(
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.HasOrganization)
        {
            return Failures.NoOrganizationContext<OrganizationResponse>();
        }

        var organizationId = CurrentUser.OrganizationId!.Value;

        if (!await IsMemberOfAsync(organizationId, cancellationToken))
        {
            return Failures.NotFoundFor<OrganizationResponse>("Organization");
        }

        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return Failures.NotFoundFor<OrganizationResponse>("Organization");
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
            return Result<OrganizationResponse>.Failure(ex.Message, Failures.Validation);
        }
    }

    public async Task<Result<OrganizationResponse>> UpdateCurrentPlanningSettingsAsync(
        UpdateOrganizationPlanningSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.HasOrganization)
        {
            return Failures.NoOrganizationContext<OrganizationResponse>();
        }

        var organizationId = CurrentUser.OrganizationId!.Value;

        if (!await IsMemberOfAsync(organizationId, cancellationToken))
        {
            return Failures.NotFoundFor<OrganizationResponse>("Organization");
        }

        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return Failures.NotFoundFor<OrganizationResponse>("Organization");
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
            return Result<OrganizationResponse>.Failure(ex.Message, Failures.Validation);
        }
    }

    public async Task<Result<IReadOnlyList<OrganizationMembershipResponse>>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return Result<IReadOnlyList<OrganizationMembershipResponse>>.Failure(
                "Not authenticated.",
                "UNAUTHORIZED");
        }

        var memberships = await _userRepository.GetByAccountIdAsync(
            CurrentUser.AccountId,
            cancellationToken);

        var names = await _organizationRepository.GetNamesByIdsAsync(
            memberships.Select(x => x.OrganizationId).Distinct().ToList(),
            cancellationToken);

        return Result<IReadOnlyList<OrganizationMembershipResponse>>.Success(
            OrganizationMapper.ToMembershipResponses(memberships, names));
    }
}
