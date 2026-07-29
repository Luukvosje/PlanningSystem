namespace Planning.Domain.Auth;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task ReplaceAsync(
        RefreshToken existingToken,
        RefreshToken replacementToken,
        CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAllForAccountAsync(Guid accountId, DateTime utcNow, CancellationToken cancellationToken = default);
}
