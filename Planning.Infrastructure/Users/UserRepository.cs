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

    public async Task<IReadOnlyList<User>> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default) =>
        await _context.Users
            .Where(x => x.AccountId == accountId && x.IsActive)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        await _context.Users
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

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> ExistsInOrganizationAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(
            x => x.AccountId == accountId && x.OrganizationId == organizationId,
            cancellationToken);
}
