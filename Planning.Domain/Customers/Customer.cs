using Planning.Domain.Common;
using Planning.Domain.Customers;

namespace Planning.Domain.Customers;

public class Customer : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; } = string.Empty;
    public string? Address { get; private set; }

    private Customer()
    {
    }

    private Customer(
        Guid id,
        Guid organizationId,
        string name,
        string email,
        string? address,
        DateTime utcNow)
        : base(id, organizationId, utcNow, utcNow)
    {
        Name = name;
        Email = email;
        Address = address;
    }

    public static Customer Create(
        Guid organizationId,
        string name,
        string email,
        string? address,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Customer name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Customer email is required.", nameof(email));
        }

        return new Customer(
            Guid.NewGuid(),
            organizationId,
            name.Trim(),
            email.Trim().ToLowerInvariant(),
            address?.Trim(),
            utcNow);
    }

    public void Update(string name, string email, string? address, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Customer name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Customer email is required.", nameof(email));
        }

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Address = address?.Trim();
        Touch(utcNow);
    }
}