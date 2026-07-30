using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.User.EventHandlers;

/// <summary>
/// Notifies both sides of an email change and writes the in-app notification. The alert goes to
/// the old address (which just lost the account) with the new address masked, so a compromised
/// old mailbox never learns it in full; the confirmation goes to the new address. Reacting to the
/// event covers every path that changes an email, closing the admin-flow notification gap by
/// construction. Both channels are handled together because they share every lookup.
/// </summary>
/// <param name="userLookupService">Lookup resolving the user's display name by id.</param>
/// <param name="mailer">Outbox mailer sending the alert and confirmation.</param>
/// <param name="notifier">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class UserEmailChangedNotificationsHandler(
    IUserLookupService userLookupService,
    IMailer mailer,
    INotifier notifier,
    ILogger<UserEmailChangedNotificationsHandler> logger
) : IDomainEventHandler<UserEmailChangedEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserEmailChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        string? userName = await userLookupService.GetUserNameByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (userName is null)
        {
            logger.LogDebug("Email change notifications skipped: user {UserId} not found.", domainEvent.UserId);
            return;
        }

        string culture = EmailCulture.Current();
        string changeTime = DateTime.UtcNow.ToString("u");
        string newEmailMasked = MaskEmail(email: domainEvent.NewEmail);

        if (domainEvent.OldEmail is not null)
        {
            await mailer.EnqueueAsync(
                template: EnumEmailTemplate.EmailChangedAlertOld,
                to: new EmailRecipient(Address: domainEvent.OldEmail, DisplayName: userName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = userName,
                    ["newEmailMasked"] = newEmailMasked,
                    ["changeTime"] = changeTime,
                },
                culture: culture,
                cancellationToken: cancellationToken
            );
        }

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.EmailChangedConfirmNew,
            to: new EmailRecipient(Address: domainEvent.NewEmail, DisplayName: userName),
            tokens: new Dictionary<string, string> { ["userName"] = userName, ["changeTime"] = changeTime },
            culture: culture,
            cancellationToken: cancellationToken
        );

        await notifier.NotifyAsync(
            userId: domainEvent.UserId,
            type: EnumNotificationType.EmailChanged,
            tokens: new Dictionary<string, string> { ["newEmailMasked"] = newEmailMasked },
            culture: culture,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Masks the local part of an address (j***@example.com), keeping only its
    /// first character.
    /// </summary>
    /// <param name="email">The address to mask.</param>
    /// <returns>The masked address.</returns>
    private static string MaskEmail(string email)
    {
        int at = email.IndexOf('@');

        if (at <= 1)
        {
            return $"***{email[at..]}";
        }

        return $"{email[0]}***{email[at..]}";
    }
}
