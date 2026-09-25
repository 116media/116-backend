using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Services;
using _116.Shared.Application.Configurations.Schemas;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace _116.Mailer.Infrastructure.Services;

/// <summary>
/// SMTP implementation of <see cref="IEmailSenderService" /> using MailKit. Covers
/// Mailpit in development (no auth, no TLS) and any authenticated relay in
/// production via <c>SMTP_*</c> configuration.
/// </summary>
public class SmtpEmailSenderService : IEmailSenderService
{
    /// <inheritdoc />
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        string host = MailEnv.SmtpHost.Value;
        int port = MailEnv.SmtpPort.Value;
        string username = MailEnv.SmtpUsername.Value;
        string password = MailEnv.SmtpPassword.Value;
        bool useStartTls = MailEnv.SmtpUseStartTls.Value;

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
        string fromAddress = MailEnv.FromAddress.Value;
        if (string.IsNullOrWhiteSpace(fromAddress))
        {
            throw new InvalidOperationException("EMAIL_FROM_ADDRESS env variable is missing or empty.");
        }

        string fromName = MailEnv.FromName.Value;

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(fromName, fromAddress));
        mime.To.Add(new MailboxAddress(message.To.DisplayName ?? string.Empty, message.To.Address));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody, TextBody = message.TextBody }.ToMessageBody();

        foreach ((string name, string value) in message.Headers ?? new Dictionary<string, string>())
        {
            mime.Headers.Add(name, value);
        }

        return mime;
    }
}
