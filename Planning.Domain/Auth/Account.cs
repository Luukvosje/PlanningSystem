using Planning.Domain.Auth;
using Planning.Domain.Common;

namespace Planning.Domain.Auth;

public class Account : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    private Account()
    {
    }

    private Account(Guid id, string email, string passwordHash, string firstName, string lastName, DateTime utcNow)
        : base(id, utcNow, utcNow)
    {
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
    }

    public static Account Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        return new Account(
            Guid.NewGuid(),
            email.Trim().ToLowerInvariant(),
            passwordHash,
            firstName.Trim(),
            lastName.Trim(),
            utcNow);
    }

    public void UpdatePasswordHash(string passwordHash, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        PasswordHash = passwordHash;
        Touch(utcNow);
    }

    public void UpdateProfile(string firstName, string lastName, string email, DateTime utcNow)
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
