using Planning.Domain.Enums;

namespace Planning.Domain.Availability;

public interface IEmployeeAvailabilityRepository
{
    Task<EmployeeAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EmployeeAvailability?> GetDayPartAsync(
        Guid organizationId,
        Guid userId,
        DateOnly date,
        DayPart dayPart,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeAvailability>> GetByUsersAndDateRangeAsync(
        Guid organizationId,
        IReadOnlyList<Guid> userIds,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        CancellationToken cancellationToken = default);

    Task AddAsync(EmployeeAvailability availability, CancellationToken cancellationToken = default);

    Task UpdateAsync(EmployeeAvailability availability, CancellationToken cancellationToken = default);

    Task DeleteAsync(EmployeeAvailability availability, CancellationToken cancellationToken = default);
}
