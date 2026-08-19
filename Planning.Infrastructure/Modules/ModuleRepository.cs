using Microsoft.EntityFrameworkCore;
using Planning.Domain.Enums;
using Planning.Domain.Modules;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Modules;

public class ModuleRepository : IModuleRepository
{
    private readonly ApplicationDbContext _context;

    public ModuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<OrganizationModule>> GetOrganizationModulesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        await _context.OrganizationModules
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Module)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserModule>> GetUserModulesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _context.UserModules
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Module)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<UserModule>>> GetUserModulesByUsersAsync(
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<UserModule>>();
        }

        var modules = await _context.UserModules
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .OrderBy(x => x.Module)
            .ToListAsync(cancellationToken);

        return modules
            .GroupBy(x => x.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<UserModule>)group.ToList());
    }

    public async Task AddOrganizationModulesAsync(
        IEnumerable<OrganizationModule> modules,
        CancellationToken cancellationToken = default)
    {
        await _context.OrganizationModules.AddRangeAsync(modules, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddUserModulesAsync(
        IEnumerable<UserModule> modules,
        CancellationToken cancellationToken = default)
    {
        await _context.UserModules.AddRangeAsync(modules, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateOrganizationModulesAsync(
        Guid organizationId,
        IReadOnlyDictionary<AppModule, bool> modules,
        CancellationToken cancellationToken = default)
    {
        // Deliberately tracked: these entries are mutated and persisted via SaveChangesAsync.
        var existing = await _context.OrganizationModules
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        foreach (var entry in existing)
        {
            if (modules.TryGetValue(entry.Module, out var isEnabled))
            {
                entry.SetEnabled(isEnabled);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUserModulesAsync(
        Guid userId,
        IReadOnlyDictionary<AppModule, bool> modules,
        CancellationToken cancellationToken = default)
    {
        // Deliberately tracked: these entries are mutated and persisted via SaveChangesAsync.
        var existing = await _context.UserModules
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var entry in existing)
        {
            if (modules.TryGetValue(entry.Module, out var isEnabled))
            {
                entry.SetEnabled(isEnabled);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
