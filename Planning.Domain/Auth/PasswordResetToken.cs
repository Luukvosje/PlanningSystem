using Planning.Domain.Common;

namespace Planning.Domain.Auth;

/// <summary>
/// A single-use, short-lived permission to set a new password without knowing the old one.
/// Only the hash is stored: a leaked database backup must not hand over working reset links.
/// </summary>
public class PasswordResetToken : BaseEntity
{
    public Guid AccountId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }

    private PasswordResetToken()
    {
    }

    private PasswordResetToken(
        Guid id,
        Guid accountId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime utcNow)
        : base(id, utcNow, utcNow)
    {
        AccountId = accountId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static PasswordResetToken Create(
        Guid accountId,
        string tokenHash,
        DateTime utcNow,
        TimeSpan validity)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("Account id is required.", nameof(accountId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Token hash is required.", nameof(tokenHash));
        }

        if (validity <= TimeSpan.Zero)
        {
            throw new ArgumentException("Validity must be positive.", nameof(validity));
        }

        return new PasswordResetToken(
            Guid.NewGuid(),
            accountId,
            tokenHash,
            utcNow.Add(validity),
            utcNow);
    }

    public bool IsUsable(DateTime utcNow) =>
        UsedAtUtc is null && utcNow < ExpiresAtUtc;

    public void MarkUsed(DateTime utcNow)
    {
        if (UsedAtUtc is not null)
        {
            throw new ArgumentException("This reset link has already been used.");
        }

        UsedAtUtc = utcNow;
        Touch(utcNow);
    }
}
