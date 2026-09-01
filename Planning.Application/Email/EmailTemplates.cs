using System.Net;
using Planning.Application.Common;

namespace Planning.Application.Email;

/// <summary>
/// Builds the outgoing messages. Every interpolated value is HTML-encoded: names and organization
/// names are user input, and an unencoded one turns a mail into an injection vector against a
/// reader whose client renders HTML.
/// </summary>
public static class EmailTemplates
{
    public static EmailMessage PasswordReset(
        string toAddress,
        string toName,
        string resetUrl,
        int validityHours)
    {
        var name = WebUtility.HtmlEncode(toName);
        var url = WebUtility.HtmlEncode(resetUrl);

        var html = Layout(
            $"""
             <p>Hallo {name},</p>
             <p>Je hebt een nieuw wachtwoord aangevraagd. Klik op de knop hieronder om er een in te stellen.</p>
             <p style="margin: 28px 0;">
               <a href="{url}" style="background:#1f2937;color:#ffffff;padding:12px 20px;border-radius:6px;text-decoration:none;display:inline-block;">Nieuw wachtwoord instellen</a>
             </p>
             <p style="color:#6b7280;font-size:14px;">Deze link verloopt over {validityHours} uur en werkt eenmalig.
             Heb je dit niet aangevraagd? Dan hoef je niets te doen - je wachtwoord blijft ongewijzigd.</p>
             <p style="color:#6b7280;font-size:14px;">Lukt de knop niet? Plak deze link in je browser:<br>{url}</p>
             """);

        var text =
            $"""
             Hallo {toName},

             Je hebt een nieuw wachtwoord aangevraagd. Open deze link om er een in te stellen:

             {resetUrl}

             Deze link verloopt over {validityHours} uur en werkt eenmalig.
             Heb je dit niet aangevraagd? Dan hoef je niets te doen - je wachtwoord blijft ongewijzigd.
             """;

        return new EmailMessage(toAddress, toName, "Nieuw wachtwoord instellen", html, text);
    }

    public static EmailMessage Invitation(
        string toAddress,
        string organizationName,
        string inviteUrl,
        DateTime expiresAtUtc)
    {
        var organization = WebUtility.HtmlEncode(organizationName);
        var url = WebUtility.HtmlEncode(inviteUrl);
        var expires = expiresAtUtc.ToString("d MMMM yyyy");

        var html = Layout(
            $"""
             <p>Hallo,</p>
             <p>Je bent uitgenodigd om deel te nemen aan <strong>{organization}</strong> in Planning.</p>
             <p style="margin: 28px 0;">
               <a href="{url}" style="background:#1f2937;color:#ffffff;padding:12px 20px;border-radius:6px;text-decoration:none;display:inline-block;">Uitnodiging accepteren</a>
             </p>
             <p style="color:#6b7280;font-size:14px;">Deze uitnodiging is geldig tot {expires}.</p>
             <p style="color:#6b7280;font-size:14px;">Lukt de knop niet? Plak deze link in je browser:<br>{url}</p>
             """);

        var text =
            $"""
             Hallo,

             Je bent uitgenodigd om deel te nemen aan {organizationName} in Planning.
             Open deze link om de uitnodiging te accepteren:

             {inviteUrl}

             Deze uitnodiging is geldig tot {expires}.
             """;

        return new EmailMessage(
            toAddress,
            toAddress,
            $"Uitnodiging voor {organizationName}",
            html,
            text);
    }

    private static string Layout(string body) =>
        $"""
         <!doctype html>
         <html lang="nl">
           <head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
           <body style="margin:0;padding:24px;background:#f9fafb;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;color:#111827;line-height:1.6;">
             <div style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:10px;padding:32px;">
               {body}
             </div>
           </body>
         </html>
         """;
}
