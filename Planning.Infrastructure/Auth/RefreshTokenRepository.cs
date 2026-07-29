using Microsoft.EntityFrameworkCore;
using Planning.Domain.Auth;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Auth;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceAsync(
        RefreshToken existingToken,
        RefreshToken replacementToken,
        CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(existingToken);
        await _context.RefreshTokens.AddAsync(replacementToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        await _context.RefreshTokens.FirstOrDefaultAsync(
            x => x.TokenHash == tokenHash,
            cancellationToken);

    public async Task RevokeAllForAccountAsync(
        Guid accountId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.AccountId == accountId && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke(utcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
