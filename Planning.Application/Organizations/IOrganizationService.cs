using Planning.Application.Auth;
using Planning.Application.Common;
using Planning.Domain.Organizations;

namespace Planning.Application.Organizations;

public sealed record CreateOrganizationResponse(OrganizationResponse Organization);

public interface IOrganizationService
{
    Task<Result<CreateOrganizationResponse>> CreateForAccountAsync(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<OrganizationResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<OrganizationResponse>> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<OrganizationMembershipResponse>>> GetMineAsync(
        CancellationToken cancellationToken = default);
}
