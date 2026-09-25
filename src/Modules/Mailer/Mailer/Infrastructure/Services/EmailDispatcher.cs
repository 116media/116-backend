using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Contracts.Application.DTOs;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace _116.Mailer.Infrastructure.Services;

/// <summary>
/// The <see cref="IEmailDispatcher" /> implementation: one enqueue per recipient the
/// message's class permits, each rendered in that recipient's own locale.
/// </summary>
/// <param name="emailService">The outbox-backed email enqueue port.</param>
/// <param name="newsletterRepository">Resolves an address's opt-in state.</param>
/// <param name="logger">Logger recording suppressed recipients.</param>
public class EmailDispatcher(
    IEmailService emailService,
    INewsletterRepository newsletterRepository,
    ILogger<EmailDispatcher> logger
) : IEmailDispatcher
{
    /// <inheritdoc />
    public async Task DispatchAsync(OutboundEmail message, CancellationToken cancellationToken = default)
    {
        foreach (EmailRecipient recipient in message.Recipients)
        {
            if (!await MayReceiveAsync(message.Class, recipient, cancellationToken))
            {
                logger.LogDebug(
                    "{Template} suppressed for a recipient: {Class} is not permitted by their opt-in state.",
                    message.TemplateName,
                    message.Class
                );
                continue;
            }

            await emailService.EnqueueAsync(
                template: message.TemplateName,
                to: new EmailRecipientDto(recipient.Address, recipient.Locale, recipient.DisplayName),
                tokens: message.Tokens,
                cancellationToken: cancellationToken
            );
        }
    }

    /// <summary>
    /// Decides whether a recipient may receive a message of this class. Security and account
    /// facts are never suppressed; courtesy mail stops at an explicit opt-out; opt-in content
    /// requires a confirmed subscription.
    /// </summary>
    /// <param name="messageClass">The message's delivery policy.</param>
    /// <param name="recipient">The resolved recipient.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> when the message may be enqueued for this recipient.</returns>
    private async Task<bool> MayReceiveAsync(
        EnumEmailClass messageClass,
        EmailRecipient recipient,
        CancellationToken cancellationToken
    )
    {
        if (messageClass is EnumEmailClass.Transactional or EnumEmailClass.Operational)
        {
            return true;
        }

        NewsletterSubscriberEntity? subscriber = await newsletterRepository.GetByEmailAsync(
            email: recipient.Address,
            cancellationToken: cancellationToken
        );

        return messageClass switch
        {
            EnumEmailClass.Subscription => subscriber?.Status == EnumNewsletterStatus.Subscribed,
            _ => subscriber?.Status != EnumNewsletterStatus.Unsubscribed,
        };
    }
}
