using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Planning.Application.Common;
using Planning.Application.Email;
using Planning.Domain.Auth;

namespace Planning.Application.Auth;

public class PasswordResetService : IPasswordResetService
{
    /// <summary>
    /// Short on purpose: a reset link is a bearer credential for an account, and it travels
    /// through a mailbox that may outlive the request.
    /// </summary>
    private static readonly TimeSpan TokenValidity = TimeSpan.FromHours(1);

    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        IAccountRepository accountRepository,
        IPasswordResetTokenRepository tokenRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IOptions<EmailOptions> emailOptions,
        ILogger<PasswordResetService> logger)
    {
        _accountRepository = accountRepository;
        _tokenRepository = tokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task<Result> RequestResetAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (account is null)
        {
            // Deliberately indistinguishable from the success path. Logged so a burst of requests
            // for non-existent addresses is still visible as the enumeration attempt it is.
            _logger.LogInformation("Password reset requested for an address without an account.");
            return Result.Success();
        }

        var utcNow = DateTime.UtcNow;

        // Requesting a new link invalidates the previous one, so a forwarded or leaked older mail
        // stops working the moment the user asks again.
        await _tokenRepository.InvalidateAllForAccountAsync(account.Id, utcNow, cancellationToken);

        var token = RefreshTokenHasher.GenerateToken();
        var resetToken = PasswordResetToken.Create(
            account.Id,
            RefreshTokenHasher.Hash(token),
            utcNow,
            TokenValidity);

        await _tokenRepository.AddAsync(resetToken, cancellationToken);

        var resetUrl = $"{_emailOptions.AppBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";

        await _emailSender.SendAsync(
            EmailTemplates.PasswordReset(
                account.Email,
                $"{account.FirstName} {account.LastName}".Trim(),
                resetUrl,
                (int)TokenValidity.TotalHours),
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ResetAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = RefreshTokenHasher.Hash(request.Token);
        var resetToken = await _tokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        var utcNow = DateTime.UtcNow;

        if (resetToken is null || !resetToken.IsUsable(utcNow))
        {
            // One message for missing, expired and already-used, so a caller cannot probe which
            // tokens ever existed.
            return Result.Failure("This reset link is no longer valid.", Failures.Validation);
        }

        var account = await _accountRepository.GetByIdAsync(resetToken.AccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure("This reset link is no longer valid.", Failures.Validation);
        }

        try
        {
            account.UpdatePasswordHash(_passwordHasher.Hash(request.NewPassword), utcNow);
            await _accountRepository.UpdateAsync(account, cancellationToken);

            resetToken.MarkUsed(utcNow);
            await _tokenRepository.UpdateAsync(resetToken, cancellationToken);

            // Whoever asked for this reset may have been locked out by someone else holding a
            // live session. Revoking every refresh token makes the reset actually reclaim the
            // account instead of merely adding a second way in.
            await _refreshTokenRepository.RevokeAllForAccountAsync(account.Id, utcNow, cancellationToken);

            _logger.LogInformation("Password reset completed for account {AccountId}.", account.Id);

            return Result.Success();
        }
        catch (ArgumentException exception)
        {
            return Result.Failure(exception.Message, Failures.Validation);
        }
    }
}
