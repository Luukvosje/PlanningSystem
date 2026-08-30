using Planning.Application.Common;
using Planning.Application.Organizations;
using Planning.Application.Modules;
using Planning.Domain.Auth;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IModuleService _moduleService;

    public AuthService(
        IAccountRepository accountRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ICurrentUserContext currentUserContext,
        IModuleService moduleService)
    {
        _accountRepository = accountRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _currentUserContext = currentUserContext;
        _moduleService = moduleService;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _accountRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Result<RegisterResponse>.Failure("Email is already registered.", "CONFLICT");
        }

        try
        {
            var passwordHash = _passwordHasher.Hash(request.Password);
            var account = Account.Create(
                request.Email,
                passwordHash,
                request.FirstName,
                request.LastName,
                DateTime.UtcNow);
            await _accountRepository.AddAsync(account, cancellationToken);

            return Result<RegisterResponse>.Success(
                new RegisterResponse(account.Id, account.Email));
        }
        catch (ArgumentException ex)
        {
            return Result<RegisterResponse>.Failure(ex.Message, Failures.Validation);
        }
    }

    public async Task<Result<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (account is null || !_passwordHasher.Verify(request.Password, account.PasswordHash))
        {
            return Result<LoginResponse>.Failure("Invalid email or password.", "UNAUTHORIZED");
        }

        var accessToken = _jwtTokenService.GenerateToken(new TokenUserContext(
            account.Id,
            account.Email));
        var refreshToken = await IssueRefreshTokenAsync(account.Id, cancellationToken);

        // Only an active membership can be landed in. An inactive one still exists and is still
        // reported by /organizations/mine - that is how the client tells a deactivated member
        // apart from an account that belongs to no organization at all.
        var memberships = (await _userRepository.GetByAccountIdAsync(account.Id, cancellationToken))
            .Where(x => x.IsActive)
            .ToList();

        if (memberships.Count == 0)
        {
            return Result<LoginResponse>.Success(
                new LoginResponse(accessToken, refreshToken, false, []));
        }

        if (memberships.Count > 1)
        {
            var membershipResponses = await BuildMembershipResponsesAsync(memberships, cancellationToken);
            return Result<LoginResponse>.Success(
                new LoginResponse(accessToken, refreshToken, true, membershipResponses));
        }

        return Result<LoginResponse>.Success(
            new LoginResponse(accessToken, refreshToken, false, null, memberships[0].OrganizationId));
    }

    public async Task<Result<TokenResponse>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<TokenResponse>.Failure("Refresh token is required.", "UNAUTHORIZED");
        }

        var tokenHash = RefreshTokenHasher.Hash(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        var utcNow = DateTime.UtcNow;

        if (existingToken is null || !existingToken.IsActive(utcNow))
        {
            return Result<TokenResponse>.Failure("Invalid or expired refresh token.", "UNAUTHORIZED");
        }

        var account = await _accountRepository.GetByIdAsync(existingToken.AccountId, cancellationToken);

        if (account is null)
        {
            return Result<TokenResponse>.Failure("Invalid or expired refresh token.", "UNAUTHORIZED");
        }

        var newRefreshTokenPlain = RefreshTokenHasher.GenerateToken();
        var newRefreshTokenHash = RefreshTokenHasher.Hash(newRefreshTokenPlain);
        var replacement = RefreshToken.Create(
            account.Id,
            newRefreshTokenHash,
            utcNow,
            _jwtTokenService.GetRefreshTokenLifetime());

        existingToken.Revoke(utcNow, newRefreshTokenHash);
        await _refreshTokenRepository.ReplaceAsync(existingToken, replacement, cancellationToken);

        var accessToken = _jwtTokenService.GenerateToken(new TokenUserContext(
            account.Id,
            account.Email));

        return Result<TokenResponse>.Success(new TokenResponse(accessToken, newRefreshTokenPlain));
    }

    public async Task<Result<CurrentUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.IsAuthenticated)
        {
            return Result<CurrentUserResponse>.Failure("Not authenticated.", "UNAUTHORIZED");
        }

        if (!_currentUserContext.HasOrganization || _currentUserContext.UserId is null)
        {
            return Result<CurrentUserResponse>.Failure(
                "No organization context. Set the X-Organization-Id header or create an organization first.",
                Failures.NoOrganization);
        }

        var user = await _userRepository.GetByIdAsync(_currentUserContext.UserId.Value, cancellationToken);

        if (user is null)
        {
            return Result<CurrentUserResponse>.Failure("User not found.", Failures.NotFound);
        }

        // Deactivated is not missing: answering NO_ORGANIZATION puts the client on the path it
        // already has for a revoked membership - drop the stale organization cookie and re-resolve
        // - instead of leaving it on a page whose every call fails.
        if (!user.IsActive)
        {
            return Result<CurrentUserResponse>.Failure(
                "Your access to this organization has been deactivated.",
                Failures.NoOrganization);
        }

        var organization = await _organizationRepository.GetByIdAsync(user.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<CurrentUserResponse>.Failure("Organization not found.", Failures.NotFound);
        }

        var modules = await _moduleService.GetEffectiveModulesAsync(
            user.Id,
            user.OrganizationId,
            cancellationToken);

        return Result<CurrentUserResponse>.Success(new CurrentUserResponse(
            user.Id,
            user.AccountId,
            user.OrganizationId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            organization.Name,
            modules));
    }

    public async Task<Result<UpdateProfileResponse>> UpdateProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.IsAuthenticated || _currentUserContext.AccountId == Guid.Empty)
        {
            return Result<UpdateProfileResponse>.Failure("Not authenticated.", "UNAUTHORIZED");
        }

        var account = await _accountRepository.GetByIdAsync(_currentUserContext.AccountId, cancellationToken);

        if (account is null)
        {
            return Result<UpdateProfileResponse>.Failure("Account not found.", Failures.NotFound);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (!string.Equals(account.Email, normalizedEmail, StringComparison.Ordinal))
        {
            var existingAccount = await _accountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

            if (existingAccount is not null && existingAccount.Id != account.Id)
            {
                return Result<UpdateProfileResponse>.Failure("Email is already registered.", "CONFLICT");
            }
        }

        try
        {
            var utcNow = DateTime.UtcNow;
            account.UpdateProfile(request.FirstName, request.LastName, request.Email, utcNow);
            await _accountRepository.UpdateAsync(account, cancellationToken);

            var memberships = await _userRepository.GetByAccountIdAsync(account.Id, cancellationToken);

            foreach (var membership in memberships)
            {
                membership.UpdateProfile(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    membership.Role,
                    utcNow);
                await _userRepository.UpdateAsync(membership, cancellationToken);
            }

            return Result<UpdateProfileResponse>.Success(new UpdateProfileResponse(
                account.Id,
                account.Email,
                account.FirstName,
                account.LastName));
        }
        catch (ArgumentException ex)
        {
            return Result<UpdateProfileResponse>.Failure(ex.Message, Failures.Validation);
        }
    }

    private async Task<string> IssueRefreshTokenAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var plainToken = RefreshTokenHasher.GenerateToken();
        var tokenHash = RefreshTokenHasher.Hash(plainToken);
        var refreshToken = RefreshToken.Create(
            accountId,
            tokenHash,
            DateTime.UtcNow,
            _jwtTokenService.GetRefreshTokenLifetime());

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        return plainToken;
    }

    private async Task<IReadOnlyList<OrganizationMembershipResponse>> BuildMembershipResponsesAsync(
        IReadOnlyList<User> memberships,
        CancellationToken cancellationToken)
    {
        var names = await _organizationRepository.GetNamesByIdsAsync(
            memberships.Select(x => x.OrganizationId).Distinct().ToList(),
            cancellationToken);

        return OrganizationMapper.ToMembershipResponses(memberships, names);
    }
}
