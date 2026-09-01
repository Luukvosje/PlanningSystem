namespace Planning.Domain.Auth;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates every outstanding token for an account. Called when a new reset is requested,
    /// so that an older link stops working, and after a successful reset.
    /// </summary>
    Task InvalidateAllForAccountAsync(Guid accountId, DateTime utcNow, CancellationToken cancellationToken = default);
}
