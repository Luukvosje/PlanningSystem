namespace Planning.Domain.Modules;

public interface IModuleRepository
{
    Task<IReadOnlyList<OrganizationModule>> GetOrganizationModulesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserModule>> GetUserModulesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Modules for several users in one query, grouped by user id. Used to render a user list
    /// without a round-trip per user.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<UserModule>>> GetUserModulesByUsersAsync(
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken = default);

    Task AddOrganizationModulesAsync(
        IEnumerable<OrganizationModule> modules,
        CancellationToken cancellationToken = default);

    Task AddUserModulesAsync(
        IEnumerable<UserModule> modules,
        CancellationToken cancellationToken = default);

    Task UpdateOrganizationModulesAsync(
        Guid organizationId,
        IReadOnlyDictionary<AppModule, bool> modules,
        CancellationToken cancellationToken = default);

    Task UpdateUserModulesAsync(
        Guid userId,
        IReadOnlyDictionary<AppModule, bool> modules,
        CancellationToken cancellationToken = default);
}
