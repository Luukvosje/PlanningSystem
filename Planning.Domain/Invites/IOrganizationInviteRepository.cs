using Planning.Domain.Invites;

namespace Planning.Domain.Invites;

public interface IOrganizationInviteRepository
{
    Task<OrganizationInvite?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(OrganizationInvite invite, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrganizationInvite invite, CancellationToken cancellationToken = default);
}
