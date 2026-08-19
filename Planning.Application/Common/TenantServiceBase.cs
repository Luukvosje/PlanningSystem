using Planning.Domain.Common;

namespace Planning.Application.Common;

/// <summary>
/// Canonical failures, so two endpoints cannot describe the same situation differently. Before
/// this existed, a missing organization context produced three different messages and two
/// different error codes depending on which service you happened to hit.
/// </summary>
public static class Failures
{
    public const string NoOrganization = "NO_ORGANIZATION";
    public const string NotFound = "NOT_FOUND";
    public const string Forbidden = "FORBIDDEN";
    public const string Validation = "VALIDATION_ERROR";

    public static Result NoOrganizationContext() =>
        Result.Failure("Organization context is required.", NoOrganization);

    public static Result<T> NoOrganizationContext<T>() =>
        Result<T>.Failure("Organization context is required.", NoOrganization);

    public static Result NotFoundFor(string subject) =>
        Result.Failure($"{subject} not found.", NotFound);

    public static Result<T> NotFoundFor<T>(string subject) =>
        Result<T>.Failure($"{subject} not found.", NotFound);

    public static Result ForbiddenFor(string message) =>
        Result.Failure(message, Forbidden);

    public static Result<T> ForbiddenFor<T>(string message) =>
        Result<T>.Failure(message, Forbidden);
}

/// <summary>
/// Shared plumbing for services that operate on tenant-owned entities.
/// <para>
/// The organization is always resolved from <see cref="ICurrentUserContext"/> - which the API
/// derives from the JWT claim - and never from a client-supplied id, because the request body is
/// attacker-controlled.
/// </para>
/// </summary>
public abstract class TenantServiceBase
{
    protected ICurrentUserContext CurrentUser { get; }

    protected TenantServiceBase(ICurrentUserContext currentUserContext) =>
        CurrentUser = currentUserContext;

    /// <summary>
    /// Resolves the current organization. Returns false when there is no organization context,
    /// in which case the caller should return <see cref="Failures.NoOrganizationContext()"/>.
    /// </summary>
    protected bool TryGetOrganizationId(out Guid organizationId)
    {
        organizationId = CurrentUser.OrganizationId ?? Guid.Empty;
        return CurrentUser.HasOrganization && CurrentUser.OrganizationId is not null;
    }

    /// <summary>
    /// Whether an entity loaded by id belongs to the current organization. A caller that gets
    /// false must answer NOT_FOUND rather than FORBIDDEN: confirming that the id exists would
    /// already leak another tenant's data.
    /// </summary>
    protected bool Owns(TenantEntity entity) =>
        CurrentUser.HasOrganization && entity.OrganizationId == CurrentUser.OrganizationId;

    /// <summary>
    /// Runs an operation that may violate a domain invariant and turns that into a validation
    /// failure. Domain entities signal invariant violations with <see cref="ArgumentException"/>;
    /// everything else is genuinely unexpected and belongs to the global exception middleware.
    /// </summary>
    protected static async Task<Result<T>> TranslateDomainErrorsAsync<T>(Func<Task<Result<T>>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ArgumentException ex)
        {
            return Result<T>.Failure(ex.Message, Failures.Validation);
        }
    }

    protected static async Task<Result> TranslateDomainErrorsAsync(Func<Task<Result>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message, Failures.Validation);
        }
    }
}
