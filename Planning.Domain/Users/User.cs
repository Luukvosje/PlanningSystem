using Planning.Domain.Auth;
using Planning.Domain.Common;
using Planning.Domain.Enums;
using Planning.Domain.Users;

namespace Planning.Domain.Users;

/// <summary>
/// A membership of an organization. The login identity is the <see cref="Auth.Account"/>; a member
/// can exist before there is one, so a planner can schedule people who have not signed up yet.
/// </summary>
public class User : TenantEntity
{
    /// <summary>Null until the member has accepted an invite and linked their login.</summary>
    public Guid? AccountId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    /// <summary>Optional for a member without an account: the planner may not know it yet.</summary>
    public string? Email { get; private set; }
    public bool HasAccount => AccountId is not null;
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
        Guid? accountId,
        Guid organizationId,
        string firstName,
        string lastName,
        string? email,
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
            NormalizeEmail(email),
            role,
            isActive: true,
            requiresApproval: false,
            utcNow);
    }

    /// <summary>
    /// A member added by a planner before the person has a login. They can be scheduled straight
    /// away; <see cref="LinkAccount"/> attaches the login once an invite is accepted.
    /// </summary>
    public static User CreateWithoutAccount(
        Guid organizationId,
        string firstName,
        string lastName,
        string? email,
        UserRole role,
        DateTime utcNow)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        return new User(
            Guid.NewGuid(),
            accountId: null,
            organizationId,
            firstName.Trim(),
            lastName.Trim(),
            NormalizeEmail(email),
            role,
            isActive: true,
            requiresApproval: false,
            utcNow);
    }

    /// <summary>
    /// Attaches the login to a member created without one. The name the planner entered stays:
    /// the planning already refers to it. The e-mail is only filled in when the planner left it
    /// empty, so an address they did enter is not silently swapped for the login's.
    /// </summary>
    public void LinkAccount(Guid accountId, string accountEmail, DateTime utcNow)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("Account id is required.", nameof(accountId));
        }

        if (HasAccount)
        {
            throw new InvalidOperationException("Member is already linked to an account.");
        }

        AccountId = accountId;
        Email ??= NormalizeEmail(accountEmail);
        Touch(utcNow);
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
        Email = NormalizeEmail(email);
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

    private static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private static void ValidateName(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Name is required.", paramName);
        }
    }
}
