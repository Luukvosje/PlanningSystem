using Microsoft.EntityFrameworkCore;
using Planning.Domain.Organizations;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Organizations;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly ApplicationDbContext _context;

    public OrganizationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Organizations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        await _context.Organizations.AddAsync(organization, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Organizations.AnyAsync(x => x.Id == id, cancellationToken);
}
