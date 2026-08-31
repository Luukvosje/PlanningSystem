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

    /// <summary>
    /// The request inbox: every rule in the organization, optionally narrowed to a set of
    /// employees. Without <paramref name="includeDecided"/> only pending requests come back.
    /// </summary>
    Task<IReadOnlyList<AvailabilityRule>> GetForOrganizationAsync(
        Guid organizationId,
        IReadOnlyList<Guid>? employeeIds,
        bool includeDecided,
        CancellationToken cancellationToken = default);

    Task AddAsync(AvailabilityRule rule, CancellationToken cancellationToken = default);

    Task UpdateAsync(AvailabilityRule rule, CancellationToken cancellationToken = default);

    Task DeleteAsync(AvailabilityRule rule, CancellationToken cancellationToken = default);
}
