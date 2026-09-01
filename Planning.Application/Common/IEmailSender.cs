namespace Planning.Application.Common;

public sealed record EmailMessage(
    string ToAddress,
    string ToName,
    string Subject,
    string HtmlBody,
    string TextBody);

public interface IEmailSender
{
    /// <summary>
    /// Delivers a message. Implementations must not throw for a delivery failure - callers send
    /// mail as a side effect of an operation that has already succeeded (a password reset was
    /// requested, an invite was created), and an unreachable mail server must not roll that back.
    /// </summary>
    Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
