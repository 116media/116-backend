using _116.Identity.Application.Shared.Messages;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Sends the security email and writes the in-app notification when a user's password changes.
/// The change origin selects both the email template and the notification type; both channels are
/// handled together because they share every lookup.
/// </summary>
/// <param name="userLookupService">Lookup resolving the recipient's name and address by id.</param>
/// <param name="messageDispatcher">Dispatcher routing the security confirmation to its recipients.</param>
/// <param name="notificationService">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped email deliveries.</param>
public class UserPasswordChangedNotificationsHandler(
    IUserLookupService userLookupService,
    IMessageDispatcher messageDispatcher,
    INotificationService notificationService,
    ILogger<UserPasswordChangedNotificationsHandler> logger
) : IDomainEventHandler<UserPasswordChangedEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserPasswordChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorDto? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user is null)
        {
            logger.LogDebug("Password change notifications skipped: user {UserId} not found.", domainEvent.UserId);
            return;
        }

        if (user.Email is not null)
        {
            var message = new PasswordChangedMessage(
                User: new MessageRecipient(
                    UserId: domainEvent.UserId,
                    Address: user.Email,
                    DisplayName: user.UserName,
                    Locale: user.PreferredLocale
                ),
                Origin: domainEvent.Origin,
                ChangedAt: DateTimeOffset.UtcNow
            );

            await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
        }
        else
        {
            logger.LogDebug("Password change email skipped: user {UserId} has no email address.", domainEvent.UserId);
        }

        await notificationService.NotifyAsync(
            userId: domainEvent.UserId,
            type: NotificationTypeFor(origin: domainEvent.Origin),
            tokens: new Dictionary<string, string>(),
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Maps the change origin to the in-app notification type shipped for that flow.
    /// </summary>
    /// <param name="origin">The flow that replaced the password.</param>
    /// <returns>The notification type to write.</returns>
    private static EnumNotificationType NotificationTypeFor(EnumPasswordChangeOrigin origin)
    {
        return origin switch
        {
            EnumPasswordChangeOrigin.Reset => EnumNotificationType.PasswordResetCompleted,
            EnumPasswordChangeOrigin.SetLocal => EnumNotificationType.LocalPasswordAdded,
            _ => EnumNotificationType.PasswordChanged,
        };
    }
}
