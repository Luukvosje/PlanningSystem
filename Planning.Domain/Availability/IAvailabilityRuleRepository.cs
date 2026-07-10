namespace Planning.Domain.Availability;

public interface IAvailabilityRuleRepository
{
    Task<AvailabilityRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AvailabilityRule>> GetByEmployeeIdAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AvailabilityRule>> GetForPlanningAsync(
        Guid organizationId,
        IReadOnlyList<Guid> employeeIds,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        CancellationToken cancellationToken = default);

    Task AddAsync(AvailabilityRule rule, CancellationToken cancellationToken = default);

    Task UpdateAsync(AvailabilityRule rule, CancellationToken cancellationToken = default);

    Task DeleteAsync(AvailabilityRule rule, CancellationToken cancellationToken = default);
}
