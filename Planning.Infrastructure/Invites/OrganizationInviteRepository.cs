using Microsoft.EntityFrameworkCore;
using Planning.Domain.Invites;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Invites;

public class OrganizationInviteRepository : IOrganizationInviteRepository
{
    private readonly ApplicationDbContext _context;

    public OrganizationInviteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrganizationInvite?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await _context.OrganizationInvites.FirstOrDefaultAsync(
            x => x.Code == code.Trim().ToUpperInvariant(),
            cancellationToken);

    public async Task AddAsync(OrganizationInvite invite, CancellationToken cancellationToken = default)
    {
        await _context.OrganizationInvites.AddAsync(invite, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(OrganizationInvite invite, CancellationToken cancellationToken = default)
    {
        _context.OrganizationInvites.Update(invite);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
