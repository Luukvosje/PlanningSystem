using Planning.Application.Common;
using Planning.Domain.Users;
using Planning.Infrastructure.Users;
using System.Security.Claims;

namespace Planning.Api.Middleware;

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
            && context.User.Identity is ClaimsIdentity identity)
        {
            var accountIdValue = context.User.FindFirstValue("accountId");
            var organizationIdValue = context.Request.Headers[OrganizationHeaders.OrganizationId].FirstOrDefault();

            if (Guid.TryParse(accountIdValue, out var accountId)
                && Guid.TryParse(organizationIdValue, out var organizationId))
            {
                var user = await userRepository.GetByAccountAndOrganizationAsync(
                    accountId,
                    organizationId,
                    context.RequestAborted);

                if (user is { IsActive: true })
                {
                    identity.AddClaim(new Claim("userId", user.Id.ToString()));
                    identity.AddClaim(new Claim("organizationId", user.OrganizationId.ToString()));
                    identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
                }
            }
        }

        await _next(context);
    }
}
