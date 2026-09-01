using Microsoft.Extensions.Logging;
using Planning.Application.Common;

namespace Planning.Infrastructure.Email;

/// <summary>
/// Used whenever no SMTP host is configured. Local development then needs no mail credentials and
/// cannot accidentally mail a real customer, while the link that would have been sent is still
/// readable in the console - which is what you need to actually test a password reset.
/// </summary>
public sealed class LogOnlyEmailSender : IEmailSender
{
    private readonly ILogger<LogOnlyEmailSender> _logger;

    public LogOnlyEmailSender(ILogger<LogOnlyEmailSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email not sent (no SMTP host configured).\nTo:      {Recipient}\nSubject: {Subject}\n{Body}",
            message.ToAddress,
            message.Subject,
            message.TextBody);

        return Task.FromResult(true);
    }
}
