using Planning.Domain.Common;
using Planning.Domain.Customers;

namespace Planning.Domain.Customers;

public class Customer : TenantEntity
{
    public const string DefaultColor = HexColor.Default;

    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; } = string.Empty;
    public string? Address { get; private set; }

    /// <summary>
    /// The colour a planning record for this customer starts with, so a week reads per customer
    /// without anyone picking a colour by hand.
    /// </summary>
    public string Color { get; private set; } = DefaultColor;

    private Customer()
    {
    }

    private Customer(
        Guid id,
        Guid organizationId,
        string name,
        string email,
        string? address,
        string color,
        DateTime utcNow)
        : base(id, organizationId, utcNow, utcNow)
    {
        Name = name;
        Email = email;
        Address = address;
        Color = color;
    }

    public static Customer Create(
        Guid organizationId,
        string name,
        string email,
        string? address,
        string? color,
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
            HexColor.Normalize(color),
            utcNow);
    }

    public void Update(string name, string email, string? address, string? color, DateTime utcNow)
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
        Color = HexColor.Normalize(color);
        Touch(utcNow);
    }
}
