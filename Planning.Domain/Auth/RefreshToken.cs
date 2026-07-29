using Planning.Domain.Common;

namespace Planning.Domain.Auth;

public class RefreshToken : BaseEntity
{
    public Guid AccountId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    private RefreshToken()
    {
    }

    private RefreshToken(
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

    public static RefreshToken Create(
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

        return new RefreshToken(
            Guid.NewGuid(),
            accountId,
            tokenHash,
            utcNow.Add(validity),
            utcNow);
    }

    public bool IsActive(DateTime utcNow) =>
        RevokedAtUtc is null && utcNow < ExpiresAtUtc;

    public void Revoke(DateTime utcNow, string? replacedByTokenHash = null)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RevokedAtUtc = utcNow;
        ReplacedByTokenHash = replacedByTokenHash;
        Touch(utcNow);
    }
}
