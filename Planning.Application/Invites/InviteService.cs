using Microsoft.Extensions.Options;
using Planning.Application.Common;
using Planning.Application.Email;
using Planning.Application.Modules;
using Planning.Domain.Auth;
using Planning.Domain.Enums;
using Planning.Domain.Invites;
using Planning.Domain.Organizations;
using Planning.Domain.Users;
using System.Security.Cryptography;

namespace Planning.Application.Invites;

public class InviteService : IInviteService
{
    private static readonly TimeSpan InviteValidity = TimeSpan.FromHours(24);

    private readonly IOrganizationInviteRepository _inviteRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IModuleService _moduleService;
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailOptions;

    public InviteService(
        IOrganizationInviteRepository inviteRepository,
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IAccountRepository accountRepository,
        ICurrentUserContext currentUserContext,
        IModuleService moduleService,
        IEmailSender emailSender,
        IOptions<EmailOptions> emailOptions)
    {
        _inviteRepository = inviteRepository;
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _currentUserContext = currentUserContext;
        _moduleService = moduleService;
        _emailSender = emailSender;
        _emailOptions = emailOptions.Value;
    }

    public async Task<Result<InviteResponse>> CreateAsync(
        CreateInviteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization || _currentUserContext.UserId is null)
        {
            return Result<InviteResponse>.Failure("Organization context is required.", Failures.NoOrganization);
        }

        if (_currentUserContext.Role is not (UserRole.Owner or UserRole.Admin))
        {
            return Result<InviteResponse>.Failure("Only owners and admins can create invites.", Failures.Forbidden);
        }

        var utcNow = DateTime.UtcNow;
        var code = GenerateInviteCode();

        var invite = OrganizationInvite.Create(
            _currentUserContext.OrganizationId!.Value,
            code,
            UserRole.Employee,
            _currentUserContext.UserId.Value,
            utcNow,
            InviteValidity);

        await _inviteRepository.AddAsync(invite, cancellationToken);

        var emailSent = false;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var organization = await _organizationRepository.GetByIdAsync(
                invite.OrganizationId,
                cancellationToken);

            var joinUrl = $"{_emailOptions.AppBaseUrl.TrimEnd('/')}/join?code={Uri.EscapeDataString(invite.Code)}";

            // The invite itself is already saved. A failed send leaves a usable code the caller
            // can still share by hand, which is why this reports the outcome instead of failing.
            emailSent = await _emailSender.SendAsync(
                EmailTemplates.Invitation(
                    request.Email.Trim(),
                    organization?.Name ?? "Planning",
                    joinUrl,
                    invite.ExpiresAtUtc),
                cancellationToken);
        }

        return Result<InviteResponse>.Success(
            new InviteResponse(invite.Code, invite.ExpiresAtUtc, emailSent));
    }

    public async Task<Result<AcceptInviteResponse>> AcceptAsync(
        AcceptInviteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.IsAuthenticated)
        {
            return Result<AcceptInviteResponse>.Failure("Not authenticated.", "UNAUTHORIZED");
        }

        var invite = await _inviteRepository.GetByCodeAsync(request.Code, cancellationToken);

        if (invite is null)
        {
            return Result<AcceptInviteResponse>.Failure("Invite not found.", Failures.NotFound);
        }

        var utcNow = DateTime.UtcNow;

        if (invite.UsedAtUtc is not null)
        {
            return Result<AcceptInviteResponse>.Failure("Invite has already been used.", "CONFLICT");
        }

        if (!invite.IsValid(utcNow))
        {
            return Result<AcceptInviteResponse>.Failure("Invite has expired.", "EXPIRED");
        }

        if (await _userRepository.ExistsInOrganizationAsync(
                _currentUserContext.AccountId,
                invite.OrganizationId,
                cancellationToken))
        {
            return Result<AcceptInviteResponse>.Failure(
                "You are already a member of this organization.",
                "CONFLICT");
        }

        var account = await _accountRepository.GetByIdAsync(_currentUserContext.AccountId, cancellationToken);

        if (account is null)
        {
            return Result<AcceptInviteResponse>.Failure("Account not found.", Failures.NotFound);
        }

        var firstName = account.FirstName;
        var lastName = account.LastName;

        try
        {
            var user = User.Create(
                _currentUserContext.AccountId,
                invite.OrganizationId,
                firstName,
                lastName,
                account.Email,
                invite.Role,
                utcNow);

            await _userRepository.AddAsync(user, cancellationToken);
            await _moduleService.InitializeUserModulesFromOrganizationAsync(
                user.Id,
                invite.OrganizationId,
                cancellationToken);
            invite.MarkUsed(user.Id, utcNow);
            await _inviteRepository.UpdateAsync(invite, cancellationToken);

            return Result<AcceptInviteResponse>.Success(
                new AcceptInviteResponse(user.Id, user.OrganizationId));
        }
        catch (ArgumentException ex)
        {
            return Result<AcceptInviteResponse>.Failure(ex.Message, Failures.Validation);
        }
        catch (InvalidOperationException ex)
        {
            return Result<AcceptInviteResponse>.Failure(ex.Message, "CONFLICT");
        }
    }

    public async Task<Result<InvitePreviewResponse>> GetPreviewAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var invite = await _inviteRepository.GetByCodeAsync(code, cancellationToken);

        if (invite is null)
        {
            return Result<InvitePreviewResponse>.Failure("Invite not found.", Failures.NotFound);
        }

        var organization = await _organizationRepository.GetByIdAsync(invite.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<InvitePreviewResponse>.Failure("Organization not found.", Failures.NotFound);
        }

        return Result<InvitePreviewResponse>.Success(new InvitePreviewResponse(
            organization.Name,
            invite.ExpiresAtUtc,
            invite.IsValid(DateTime.UtcNow)));
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);

        var code = new char[8];
        for (var i = 0; i < 8; i++)
        {
            code[i] = chars[bytes[i] % chars.Length];
        }

        return new string(code);
    }
}
