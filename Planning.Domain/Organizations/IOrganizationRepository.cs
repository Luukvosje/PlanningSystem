using Planning.Domain.Organizations;

namespace Planning.Domain.Organizations;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
