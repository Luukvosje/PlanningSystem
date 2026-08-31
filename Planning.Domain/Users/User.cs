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

    /// <summary>
    /// Whether this member's own availability and leave has to be approved by a planner before it
    /// counts. Only honoured for <see cref="UserRole.Employee"/>: anyone who may approve requests
    /// would otherwise be approving their own.
    /// </summary>
    public bool RequiresApproval { get; private set; }

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
        bool requiresApproval,
        DateTime utcNow)
        : base(id, organizationId, utcNow, utcNow)
    {
        AccountId = accountId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Role = role;
        IsActive = isActive;
        RequiresApproval = requiresApproval;
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
            requiresApproval: false,
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

        // Anyone but an employee decides on requests themselves, so the flag would be silently
        // ignored from here on. Clearing it keeps the stored value from lying about what happens.
        if (role != UserRole.Employee)
        {
            RequiresApproval = false;
        }

        Touch(utcNow);
    }

    public void SetRequiresApproval(bool requiresApproval, DateTime utcNow)
    {
        if (requiresApproval && Role != UserRole.Employee)
        {
            throw new ArgumentException("Only an employee can be required to request availability.");
        }

        RequiresApproval = requiresApproval;
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
