using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace _116.Mailer.Infrastructure.Services;

/// <summary>
/// SMTP implementation of <see cref="IEmailSender" /> using MailKit. Covers
/// Mailpit in development (no auth, no TLS) and any authenticated relay in
/// production via <c>SMTP_*</c> configuration.
/// </summary>
/// <param name="configuration">The configuration providing SMTP and sender settings.</param>
public class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    /// <inheritdoc />
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        string host = configuration["SMTP_HOST"] ?? "localhost";
        int port = int.TryParse(configuration["SMTP_PORT"], out int parsed) ? parsed : 1025;
        string username = configuration["SMTP_USERNAME"] ?? string.Empty;
        string password = configuration["SMTP_PASSWORD"] ?? string.Empty;
        bool useStartTls = bool.TryParse(configuration["SMTP_USE_STARTTLS"], out bool tls) && tls;

        MimeMessage mime = BuildMime(message);

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(
                host,
                port,
                useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken
            );

            if (username.Length > 0)
            {
                await client.AuthenticateAsync(username, password, cancellationToken);
            }

            await client.SendAsync(mime, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);
        }
        catch (SmtpCommandException exception)
        {
            // 5xx replies are the server refusing the message outright
            // (bad recipient, rejected sender) — retrying cannot help.
            bool isPermanent = (int)exception.StatusCode >= 500;
            throw new EmailDeliveryException(
                message: $"SMTP command failed ({(int)exception.StatusCode}): {exception.Message}",
                isTransient: !isPermanent
            );
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new EmailDeliveryException(message: $"SMTP delivery failed: {exception.Message}");
        }
    }

    /// <summary>
    /// Builds the MIME message: configured sender identity, the recipient, and
    /// a multipart/alternative body carrying both the text and HTML parts.
    /// </summary>
    private MimeMessage BuildMime(EmailMessage message)
    {
        string fromAddress =
            configuration["EMAIL_FROM_ADDRESS"]
            ?? throw new InvalidOperationException("EMAIL_FROM_ADDRESS env variable is missing or empty.");
        string fromName = configuration["EMAIL_FROM_NAME"] ?? "116";

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(fromName, fromAddress));
        mime.To.Add(new MailboxAddress(message.To.DisplayName ?? string.Empty, message.To.Address));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody, TextBody = message.TextBody }.ToMessageBody();

        return mime;
    }
}
