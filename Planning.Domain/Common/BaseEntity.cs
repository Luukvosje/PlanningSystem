namespace Planning.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime UpdatedAtUtc { get; protected set; }

    protected BaseEntity()
    {
    }

    protected BaseEntity(Guid id, DateTime createdAtUtc, DateTime updatedAtUtc)
    {
        Id = id;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    protected void Touch(DateTime utcNow)
    {
        UpdatedAtUtc = utcNow;
    }
}
