using Microsoft.EntityFrameworkCore;
using Planning.Domain.Enums;
using Planning.Domain.Planning;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Planning;

public class PlanningRecordRepository : IPlanningRecordRepository
{
    private readonly ApplicationDbContext _context;

    public PlanningRecordRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlanningRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.PlanningRecords.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PlanningRecord>> GetByOrganizationAndWeekAsync(
        Guid organizationId,
        DateTime weekStartUtc,
        DateTime weekEndUtc,
        CancellationToken cancellationToken = default) =>
        await BuildRangeQuery(organizationId, weekStartUtc, weekEndUtc, null, null, null, null)
            .OrderBy(x => x.StartUtc)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<PlanningRecord> Items, int TotalCount)> GetByOrganizationAndRangeAsync(
        Guid organizationId,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc,
        IReadOnlyList<Guid>? userIds,
        IReadOnlyList<Guid>? customerIds,
        IReadOnlyList<PlanningStatus>? statuses,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = BuildRangeQuery(
            organizationId,
            rangeStartUtc,
            rangeEndUtc,
            userIds,
            customerIds,
            statuses,
            search);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.StartUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(PlanningRecord planningRecord, CancellationToken cancellationToken = default)
    {
        await _context.PlanningRecords.AddAsync(planningRecord, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PlanningRecord planningRecord, CancellationToken cancellationToken = default)
    {
        _context.PlanningRecords.Update(planningRecord);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(PlanningRecord planningRecord, CancellationToken cancellationToken = default)
    {
        _context.PlanningRecords.Remove(planningRecord);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.PlanningRecords.AnyAsync(x => x.Id == id, cancellationToken);

    private IQueryable<PlanningRecord> BuildRangeQuery(
        Guid organizationId,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc,
        IReadOnlyList<Guid>? userIds,
        IReadOnlyList<Guid>? customerIds,
        IReadOnlyList<PlanningStatus>? statuses,
        string? search)
    {
        var query = _context.PlanningRecords
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.StartUtc < rangeEndUtc &&
                x.EndUtc > rangeStartUtc);

        if (userIds is { Count: > 0 })
        {
            query = query.Where(x => userIds.Contains(x.AssignedUserId));
        }

        if (customerIds is { Count: > 0 })
        {
            query = query.Where(x => x.CustomerId.HasValue && customerIds.Contains(x.CustomerId.Value));
        }

        if (statuses is { Count: > 0 })
        {
            query = query.Where(x => statuses.Contains(x.Status));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Title.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)) ||
                (x.Notes != null && x.Notes.Contains(term)));
        }

        return query;
    }
}
