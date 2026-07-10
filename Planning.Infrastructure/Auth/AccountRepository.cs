using Microsoft.EntityFrameworkCore;
using Planning.Domain.Auth;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Auth;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Accounts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Accounts.FirstOrDefaultAsync(
            x => x.Email == email.Trim().ToLowerInvariant(),
            cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Accounts.AnyAsync(
            x => x.Email == email.Trim().ToLowerInvariant(),
            cancellationToken);
}
