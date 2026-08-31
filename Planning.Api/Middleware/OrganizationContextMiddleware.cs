using Planning.Application.Common;
using Planning.Domain.Users;
using System.Security.Claims;

namespace Planning.Api.Middleware;

/// <summary>
/// Turns the account behind the JWT into a membership of one organization, which is what every
/// authorization policy and tenant-owned query downstream actually runs on.
/// <para>
/// The organization is never taken from the request body: the header is only a *request* for one
/// of the account's own memberships, and is honoured solely when that membership exists and is
/// active.
/// </para>
/// </summary>
public class OrganizationContextMiddleware
{
    private readonly RequestDelegate _next;

    public OrganizationContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && context.User.Identity is ClaimsIdentity identity
            && Guid.TryParse(context.User.FindFirstValue("accountId"), out var accountId))
        {
            var membership = await ResolveMembershipAsync(
                userRepository,
                accountId,
                context.Request.Headers[OrganizationHeaders.OrganizationId].FirstOrDefault(),
                context.RequestAborted);

            if (membership is not null)
            {
                identity.AddClaim(new Claim("userId", membership.Id.ToString()));
                identity.AddClaim(new Claim("organizationId", membership.OrganizationId.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, membership.Role.ToString()));
            }
        }

        await _next(context);
    }

    /// <summary>
    /// The requested organization when the header names one the account may still enter,
    /// otherwise the account's only active membership.
    /// <para>
    /// Falling back matters because the header is client state that is routinely absent - a first
    /// call after login, a cleared cookie, a script or a second client - and without it every
    /// endpoint answered NO_ORGANIZATION for a user who belongs to exactly one organization and
    /// had nothing to choose. An account with several active memberships is deliberately left
    /// without a context instead: picking one of them here would silently decide which tenant the
    /// caller reads and writes, so that choice stays with the client (see /organizations/mine).
    /// </para>
    /// </summary>
    private static async Task<User?> ResolveMembershipAsync(
        IUserRepository userRepository,
        Guid accountId,
        string? requestedOrganizationId,
        CancellationToken cancellationToken)
    {
        if (Guid.TryParse(requestedOrganizationId, out var organizationId))
        {
            var requested = await userRepository.GetByAccountAndOrganizationAsync(
                accountId,
                organizationId,
                cancellationToken);

            if (requested is { IsActive: true })
            {
                return requested;
            }
        }

        var active = (await userRepository.GetByAccountIdAsync(accountId, cancellationToken))
            .Where(membership => membership.IsActive)
            .Take(2)
            .ToList();

        return active.Count == 1 ? active[0] : null;
    }
}
