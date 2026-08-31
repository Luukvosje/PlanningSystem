using Microsoft.EntityFrameworkCore;
using Planning.Domain.Availability;
using Planning.Domain.Enums;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Availability;

public class AvailabilityRuleRepository : IAvailabilityRuleRepository
{
    private readonly ApplicationDbContext _context;

    public AvailabilityRuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AvailabilityRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.AvailabilityRules
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AvailabilityRule>> GetByEmployeeIdAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        await _context.AvailabilityRules
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.EmployeeId == employeeId)
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Weekday)
            .ThenBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AvailabilityRule>> GetForPlanningAsync(
        Guid organizationId,
        IReadOnlyList<Guid> employeeIds,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        CancellationToken cancellationToken = default)
    {
        if (employeeIds.Count == 0)
        {
            return [];
        }

        return await _context.AvailabilityRules
            .AsNoTracking()
            .Where(x =>
                x.OrganizationId == organizationId
                && employeeIds.Contains(x.EmployeeId)
                && x.ApprovalStatus != ApprovalStatus.Rejected
                && (x.Type == AvailabilityRuleType.Weekly
                    || (x.Type == AvailabilityRuleType.OneTime
                        && x.Date >= rangeStart
                        && x.Date <= rangeEnd)))
            .OrderBy(x => x.EmployeeId)
            .ThenBy(x => x.Type)
            .ThenBy(x => x.Weekday)
            .ThenBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AvailabilityRule>> GetForOrganizationAsync(
        Guid organizationId,
        IReadOnlyList<Guid>? employeeIds,
        bool includeDecided,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AvailabilityRules
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId);

        if (employeeIds is not null)
        {
            query = query.Where(x => employeeIds.Contains(x.EmployeeId));
        }

        if (!includeDecided)
        {
            query = query.Where(x => x.ApprovalStatus == ApprovalStatus.Pending);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AvailabilityRule rule, CancellationToken cancellationToken = default)
    {
        await _context.AvailabilityRules.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AvailabilityRule rule, CancellationToken cancellationToken = default)
    {
        _context.AvailabilityRules.Update(rule);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(AvailabilityRule rule, CancellationToken cancellationToken = default)
    {
        _context.AvailabilityRules.Remove(rule);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
