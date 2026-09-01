using Microsoft.EntityFrameworkCore;
using Planning.Domain.Auth;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Auth;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        await _context.PasswordResetTokens.AddAsync(token, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PasswordResetToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        await _context.PasswordResetTokens.FirstOrDefaultAsync(
            x => x.TokenHash == tokenHash,
            cancellationToken);

    public async Task InvalidateAllForAccountAsync(
        Guid accountId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var tokens = await _context.PasswordResetTokens
            .Where(x => x.AccountId == accountId && x.UsedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.MarkUsed(utcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
