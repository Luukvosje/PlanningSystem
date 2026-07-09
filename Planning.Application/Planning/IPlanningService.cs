using Planning.Application.Common;

namespace Planning.Application.Planning;

public interface IPlanningService
{
    Task<Result<PlanningResponse>> CreateAsync(CreatePlanningRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlanningResponse>> UpdateAsync(Guid id, UpdatePlanningRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlanningResponse>> MoveAsync(Guid id, MovePlanningRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlanningResponse>> DuplicateAsync(Guid id, DuplicatePlanningRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PlanningResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PlanningListResponse>> GetListAsync(PlanningListRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlanningResponse>>> GetWeekPlanningAsync(WeekPlanningRequest request, CancellationToken cancellationToken = default);
}
