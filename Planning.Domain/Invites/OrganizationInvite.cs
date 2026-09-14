using Planning.Domain.Common;
using Planning.Domain.Enums;
using Planning.Domain.Invites;

namespace Planning.Domain.Invites;

public class OrganizationInvite : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    /// <summary>
    /// The member this invite links a login to. Null for an open invite, which creates a new
    /// member for whoever accepts it.
    /// </summary>
    public Guid? UserId { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }
    public Guid? UsedByUserId { get; private set; }

    private OrganizationInvite()
    {
    }

    private OrganizationInvite(
        Guid id,
        Guid organizationId,
        string code,
        UserRole role,
        Guid createdByUserId,
        Guid? userId,
        DateTime expiresAtUtc,
        DateTime utcNow)
        : base(id, utcNow, utcNow)
    {
        OrganizationId = organizationId;
        Code = code;
        Role = role;
        CreatedByUserId = createdByUserId;
        UserId = userId;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static OrganizationInvite Create(
        Guid organizationId,
        string code,
        UserRole role,
        Guid createdByUserId,
        DateTime utcNow,
        TimeSpan validity,
        Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code is required.", nameof(code));
        }

        return new OrganizationInvite(
            Guid.NewGuid(),
            organizationId,
            code.Trim().ToUpperInvariant(),
            role,
            createdByUserId,
            userId,
            utcNow.Add(validity),
            utcNow);
    }

    public bool IsValid(DateTime utcNow) =>
        UsedAtUtc is null && utcNow < ExpiresAtUtc;

    public void MarkUsed(Guid userId, DateTime utcNow)
    {
        if (UsedAtUtc is not null)
        {
            throw new InvalidOperationException("Invite has already been used.");
        }

        if (utcNow >= ExpiresAtUtc)
        {
            throw new InvalidOperationException("Invite has expired.");
        }

        UsedAtUtc = utcNow;
        UsedByUserId = userId;
        Touch(utcNow);
    }
}
