using _116.Identity.Application.Shared.Messages;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
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
/// <param name="emailService">Outbox mailer sending the alert and confirmation.</param>
/// <param name="notificationService">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class UserEmailChangedNotificationsHandler(
    IUserLookupService userLookupService,
    IMessageDispatcher messageDispatcher,
    INotificationService notificationService,
    ILogger<UserEmailChangedNotificationsHandler> logger
) : IDomainEventHandler<UserEmailChangedEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserEmailChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorDto? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user is null)
        {
            logger.LogDebug("Email change notifications skipped: user {UserId} not found.", domainEvent.UserId);
            return;
        }

        DateTimeOffset changedAt = DateTimeOffset.UtcNow;
        string newEmailMasked = MaskEmail(email: domainEvent.NewEmail);

        if (domainEvent.OldEmail is not null)
        {
            var alert = new EmailChangedAlertMessage(
                FormerAddress: new MessageRecipient(
                    UserId: domainEvent.UserId,
                    Address: domainEvent.OldEmail,
                    DisplayName: user.UserName,
                    Locale: user.PreferredLocale
                ),
                NewEmailMasked: newEmailMasked,
                ChangedAt: changedAt
            );

            await messageDispatcher.DispatchAsync(message: alert, cancellationToken: cancellationToken);
        }

        var confirmation = new EmailChangedConfirmationMessage(
            NewAddress: new MessageRecipient(
                UserId: domainEvent.UserId,
                Address: domainEvent.NewEmail,
                DisplayName: user.UserName,
                Locale: user.PreferredLocale
            ),
            ChangedAt: changedAt
        );

        await messageDispatcher.DispatchAsync(message: confirmation, cancellationToken: cancellationToken);

        await notificationService.NotifyAsync(
            userId: domainEvent.UserId,
            type: EnumNotificationType.EmailChanged,
            tokens: new Dictionary<string, string> { ["newEmailMasked"] = newEmailMasked },
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
