using Planning.Domain.Enums;

namespace Planning.Application.Common;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    Guid AccountId { get; }
    Guid? OrganizationId { get; }
    string Email { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool HasOrganization { get; }
}
