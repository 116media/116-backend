using _116.Mailer.Contracts.Application;

namespace _116.Mailer.Application.Shared.Services;

/// <summary>
/// A fully rendered, provider-agnostic email ready for transport. The sender
/// identity is deliberately absent: it is environment configuration applied by
/// the adapter, so business code cannot spoof it.
/// </summary>
/// <param name="To">The recipient of the email.</param>
/// <param name="Subject">The rendered subject line.</param>
/// <param name="HtmlBody">The rendered HTML body.</param>
/// <param name="TextBody">The rendered plain-text body.</param>
public record EmailMessage(EmailRecipient To, string Subject, string HtmlBody, string TextBody);

/// <summary>
/// Transport seam to a concrete email provider. Implementations perform one
/// delivery attempt and surface failures as
/// <see cref="Exceptions.EmailDeliveryException" />; retry policy belongs to
/// the caller, never to the adapter.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Performs a single delivery attempt for the given message.
    /// </summary>
    /// <param name="message">The fully rendered email to deliver.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
