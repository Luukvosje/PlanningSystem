using Planning.Application.Common;

namespace Planning.Application.Availability;

public interface IAvailabilityService
{
    Task<Result<WeekAvailabilityResponse>> GetWeekAsync(
        WeekAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AvailabilityResponse?>> UpsertDayPartAsync(
        UpsertDayPartAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AvailabilityResponse>> UpsertTimeBlockAsync(
        UpsertTimeBlockAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
