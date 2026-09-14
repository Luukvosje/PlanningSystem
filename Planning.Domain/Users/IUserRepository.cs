using Planning.Domain.Users;

namespace Planning.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByAccountAndOrganizationAsync(Guid accountId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsInOrganizationAsync(Guid accountId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithEmailAsync(Guid organizationId, string email, CancellationToken cancellationToken = default);
}
