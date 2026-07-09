namespace Planning.Domain.Common;

public abstract class TenantEntity : BaseEntity
{
    public Guid OrganizationId { get; protected set; }

    protected TenantEntity()
    {
    }

    protected TenantEntity(Guid id, Guid organizationId, DateTime createdAtUtc, DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        OrganizationId = organizationId;
    }
}
