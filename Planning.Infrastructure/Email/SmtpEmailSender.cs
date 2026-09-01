using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Planning.Application.Common;

namespace Planning.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mime.To.Add(new MailboxAddress(message.ToName, message.ToAddress));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody,
        }.ToMessageBody();

        try
        {
            using var client = new SmtpClient();

            // Explicit rather than StartTlsWhenAvailable: "when available" silently accepts a
            // plaintext session if a man in the middle strips the STARTTLS capability, which would
            // hand over the SMTP password and the contents of every password-reset mail.
            var security = _options.SmtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, security, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            }

            await client.SendAsync(mime, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Sent '{Subject}' to {Recipient}.", message.Subject, message.ToAddress);
            return true;
        }
        catch (Exception exception)
        {
            // Swallowed on purpose - see IEmailSender. The caller's operation already succeeded and
            // must not be rolled back because a mail server was briefly unreachable. Logged at
            // Error so an outage is visible rather than silent.
            _logger.LogError(
                exception,
                "Failed to send '{Subject}' to {Recipient}.",
                message.Subject,
                message.ToAddress);
            return false;
        }
    }
}
