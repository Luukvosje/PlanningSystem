using Planning.Domain.Auth;
using Planning.Domain.Common;
using Planning.Domain.Enums;
using Planning.Domain.Users;

namespace Planning.Domain.Users;

public class User : TenantEntity
{
    public Guid AccountId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    private User()
    {
    }

    private User(
        Guid id,
        Guid accountId,
        Guid organizationId,
        string firstName,
        string lastName,
        string email,
        UserRole role,
        bool isActive,
        DateTime utcNow)
        : base(id, organizationId, utcNow, utcNow)
    {
        AccountId = accountId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Role = role;
        IsActive = isActive;
    }

    public static User Create(
        Guid accountId,
        Guid organizationId,
        string firstName,
        string lastName,
        string email,
        UserRole role,
        DateTime utcNow)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("Account id is required.", nameof(accountId));
        }

        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        return new User(
            Guid.NewGuid(),
            accountId,
            organizationId,
            firstName.Trim(),
            lastName.Trim(),
            email.Trim().ToLowerInvariant(),
            role,
            isActive: true,
            utcNow);
    }

    public void UpdateProfile(string firstName, string lastName, string email, UserRole role, DateTime utcNow)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Role = role;
        Touch(utcNow);
    }

    public void ChangeRole(UserRole role, DateTime utcNow)
    {
        Role = role;
        Touch(utcNow);
    }

    public void Activate(DateTime utcNow)
    {
        IsActive = true;
        Touch(utcNow);
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        Touch(utcNow);
    }

    private static void ValidateName(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Name is required.", paramName);
        }
    }
}
