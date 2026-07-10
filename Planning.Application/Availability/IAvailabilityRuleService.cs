using Planning.Application.Common;
using Planning.Domain.Availability;
using Planning.Domain.Enums;
using Planning.Domain.Users;

namespace Planning.Application.Availability;

public interface IAvailabilityRuleService
{
    Task<Result<AvailabilityRulesListResponse>> ListByEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<Result<PlanningAvailabilityResponse>> GetForPlanningAsync(
        PlanningAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AvailabilityRuleResponse>> CreateAsync(
        CreateAvailabilityRuleRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AvailabilityRuleResponse>> UpdateAsync(
        Guid id,
        UpdateAvailabilityRuleRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
