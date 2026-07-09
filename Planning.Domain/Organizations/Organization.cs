using Planning.Domain.Common;
using Planning.Domain.Organizations;

namespace Planning.Domain.Organizations;
public class Organization : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    private Organization()
    {
    }

    private Organization(Guid id, string name, string email, DateTime utcNow)
        : base(id, utcNow, utcNow)
    {
        Name = name;
        Email = email;
    }

    public static Organization Create(string name, string email, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Organization name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Organization email is required.", nameof(email));
        }

        return new Organization(Guid.NewGuid(), name.Trim(), email.Trim().ToLowerInvariant(), utcNow);
    }

    public void Update(string name, string email, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Organization name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Organization email is required.", nameof(email));
        }

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Touch(utcNow);
    }
}
