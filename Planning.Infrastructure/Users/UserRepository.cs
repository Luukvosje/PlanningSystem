using Microsoft.EntityFrameworkCore;
using Planning.Domain.Users;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Users;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<User?> GetByAccountAndOrganizationAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        await _context.Users.FirstOrDefaultAsync(
            x => x.AccountId == accountId && x.OrganizationId == organizationId,
            cancellationToken);

    /// <summary>
    /// Every membership of the account, deactivated ones included. Callers decide what an
    /// inactive membership means to them: login skips it, /organizations/mine reports it so the
    /// client can tell "deactivated" apart from "belongs to no organization".
    /// </summary>
    public async Task<IReadOnlyList<User>> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default) =>
        await _context.Users
            .AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        await _context.Users
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsInOrganizationAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(
            x => x.AccountId == accountId && x.OrganizationId == organizationId,
            cancellationToken);

    public async Task<bool> ExistsWithEmailAsync(
        Guid organizationId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Users.AnyAsync(
            x => x.OrganizationId == organizationId && x.Email == normalized,
            cancellationToken);
    }
}
