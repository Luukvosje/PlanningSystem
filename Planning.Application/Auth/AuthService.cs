using Planning.Application.Common;
using Planning.Application.Modules;
using Planning.Domain.Auth;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IModuleService _moduleService;

    public AuthService(
        IAccountRepository accountRepository,
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ICurrentUserContext currentUserContext,
        IModuleService moduleService)
    {
        _accountRepository = accountRepository;
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
            return Result<RegisterResponse>.Failure(ex.Message, "VALIDATION_ERROR");
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

        var token = _jwtTokenService.GenerateToken(new TokenUserContext(
            account.Id,
            account.Email));

        var memberships = await _userRepository.GetByAccountIdAsync(account.Id, cancellationToken);

        if (memberships.Count == 0)
        {
            return Result<LoginResponse>.Success(
                new LoginResponse(token, false, []));
        }

        if (memberships.Count > 1)
        {
            var membershipResponses = await BuildMembershipResponsesAsync(memberships, cancellationToken);
            return Result<LoginResponse>.Success(
                new LoginResponse(token, true, membershipResponses));
        }

        return Result<LoginResponse>.Success(
            new LoginResponse(token, false, null, memberships[0].OrganizationId));
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
                "NO_ORGANIZATION");
        }

        var user = await _userRepository.GetByIdAsync(_currentUserContext.UserId.Value, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Result<CurrentUserResponse>.Failure("User not found.", "NOT_FOUND");
        }

        var organization = await _organizationRepository.GetByIdAsync(user.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<CurrentUserResponse>.Failure("Organization not found.", "NOT_FOUND");
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
            return Result<UpdateProfileResponse>.Failure("Account not found.", "NOT_FOUND");
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
            return Result<UpdateProfileResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    private async Task<IReadOnlyList<OrganizationMembershipResponse>> BuildMembershipResponsesAsync(
        IReadOnlyList<User> memberships,
        CancellationToken cancellationToken)
    {
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

        return responses;
    }
}
