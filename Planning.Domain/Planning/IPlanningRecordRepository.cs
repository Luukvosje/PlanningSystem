using Planning.Domain.Enums;
using Planning.Domain.Planning;

namespace Planning.Domain.Planning;

public interface IPlanningRecordRepository
{
    Task<PlanningRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<PlanningRecord> Items, int TotalCount)> GetByOrganizationAndRangeAsync(
        Guid organizationId,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc,
        IReadOnlyList<Guid>? userIds,
        IReadOnlyList<Guid>? customerIds,
        IReadOnlyList<PlanningStatus>? statuses,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(PlanningRecord planningRecord, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlanningRecord planningRecord, CancellationToken cancellationToken = default);
    Task DeleteAsync(PlanningRecord planningRecord, CancellationToken cancellationToken = default);
}
