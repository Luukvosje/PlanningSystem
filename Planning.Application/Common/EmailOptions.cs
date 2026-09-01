namespace Planning.Application.Common;

/// <summary>
/// Bound from the "Email" configuration section. In production every value arrives as an
/// environment variable (Email__SmtpHost and friends) from deploy/.env - none of it belongs in
/// a committed appsettings file.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Must live on a domain whose SPF and DKIM records point at the sending provider. Get this
    /// wrong and mail is accepted by the provider and then silently dropped by the recipient.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Planning";

    /// <summary>
    /// Public base URL of the frontend, used to build the links in outgoing mail. Without a
    /// correct value the links point at localhost and the mail is useless.
    /// </summary>
    public string AppBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// When true, messages are written to the log instead of being sent. This is the default
    /// whenever no SMTP host is configured, so local development never needs mail credentials
    /// and never accidentally mails a real person.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(SmtpHost)
        && !string.IsNullOrWhiteSpace(FromAddress);
}
