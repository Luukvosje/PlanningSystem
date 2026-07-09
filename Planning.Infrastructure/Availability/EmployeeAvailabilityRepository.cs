using Microsoft.EntityFrameworkCore;
using Planning.Domain.Availability;
using Planning.Domain.Enums;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Availability;

public class EmployeeAvailabilityRepository : IEmployeeAvailabilityRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeAvailabilityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.EmployeeAvailabilities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<EmployeeAvailability?> GetDayPartAsync(
        Guid organizationId,
        Guid userId,
        DateOnly date,
        DayPart dayPart,
        CancellationToken cancellationToken = default) =>
        await _context.EmployeeAvailabilities.FirstOrDefaultAsync(
            x =>
                x.OrganizationId == organizationId &&
                x.UserId == userId &&
                x.Date == date &&
                x.Type == AvailabilityType.DayPart &&
                x.DayPart == dayPart,
            cancellationToken);

    public async Task<IReadOnlyList<EmployeeAvailability>> GetByUsersAndDateRangeAsync(
        Guid organizationId,
        IReadOnlyList<Guid> userIds,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        CancellationToken cancellationToken = default)
    {
        var query = _context.EmployeeAvailabilities
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.Date >= rangeStart &&
                x.Date <= rangeEnd);

        if (userIds is { Count: > 0 })
        {
            query = query.Where(x => userIds.Contains(x.UserId));
        }

        return await query
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Type)
            .ThenBy(x => x.DayPart)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EmployeeAvailability availability, CancellationToken cancellationToken = default)
    {
        await _context.EmployeeAvailabilities.AddAsync(availability, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmployeeAvailability availability, CancellationToken cancellationToken = default)
    {
        _context.EmployeeAvailabilities.Update(availability);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(EmployeeAvailability availability, CancellationToken cancellationToken = default)
    {
        _context.EmployeeAvailabilities.Remove(availability);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
