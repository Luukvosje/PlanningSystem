using Planning.Domain.Enums;

namespace Planning.Application.Common;

public sealed record TokenUserContext(
    Guid AccountId,
    string Email,
    Guid? UserId = null,
    Guid? OrganizationId = null,
    UserRole? Role = null);

public interface IJwtTokenService
{
    string GenerateToken(TokenUserContext context);
    TimeSpan GetRefreshTokenLifetime();
}
