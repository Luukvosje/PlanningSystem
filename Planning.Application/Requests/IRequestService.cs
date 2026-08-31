using Planning.Application.Common;

namespace Planning.Application.Requests;

public interface IRequestService
{
    Task<Result<RequestListResponse>> ListAsync(
        bool includeDecided,
        CancellationToken cancellationToken = default);

    Task<Result<DecideRequestsResponse>> ApproveAsync(
        DecideRequestsRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<DecideRequestsResponse>> RejectAsync(
        DecideRequestsRequest request,
        CancellationToken cancellationToken = default);
}
