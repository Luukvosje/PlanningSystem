using Planning.Application.Common;
using Planning.Domain.Enums;
using Planning.Domain.Users;
using System.Security.Claims;

namespace Planning.Api.Services;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public Guid AccountId => GetRequiredGuid("accountId");

    public Guid? UserId => GetOptionalGuid("userId");

    public Guid? OrganizationId => GetOptionalGuid("organizationId");

    public string Email =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("email")
        ?? string.Empty;

    public UserRole? Role
    {
        get
        {
            var roleValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

            return Enum.TryParse<UserRole>(roleValue, out var role)
                ? role
                : null;
        }
    }

    public bool HasOrganization => OrganizationId.HasValue && UserId.HasValue;

    private Guid GetRequiredGuid(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);

        return Guid.TryParse(value, out var id)
            ? id
            : Guid.Empty;
    }

    private Guid? GetOptionalGuid(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);

        return Guid.TryParse(value, out var id)
            ? id
            : null;
    }
}
