using _116.Identity.Application.Shared.Messages;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Sends the security email and writes the in-app notification when every session on an account
/// is terminated at once. The admin-driven variant uses the force-logout copy; the self-service
/// variant the signed-out-everywhere copy. Both channels are handled together because they share
/// every lookup.
/// </summary>
/// <param name="userLookupService">Lookup resolving the recipient's name and address by id.</param>
/// <param name="emailService">Outbox mailer sending the security notice.</param>
/// <param name="notificationService">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped email deliveries.</param>
public class UserSignedOutAllDevicesNotificationsHandler(
    IUserLookupService userLookupService,
    IMessageDispatcher messageDispatcher,
    INotificationService notificationService,
    ILogger<UserSignedOutAllDevicesNotificationsHandler> logger
) : IDomainEventHandler<UserSignedOutAllDevicesEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserSignedOutAllDevicesEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorDto? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user is null)
        {
            logger.LogDebug("Mass sign-out notifications skipped: user {UserId} not found.", domainEvent.UserId);
            return;
        }

        if (user.Email is not null)
        {
            var message = new SignedOutAllDevicesMessage(
                User: new MessageRecipient(
                    UserId: domainEvent.UserId,
                    Address: user.Email,
                    DisplayName: user.UserName,
                    Locale: user.PreferredLocale
                ),
                ByAdmin: domainEvent.ByAdmin,
                SignedOutAt: DateTimeOffset.UtcNow
            );

            await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
        }
        else
        {
            logger.LogDebug("Mass sign-out email skipped: user {UserId} has no email address.", domainEvent.UserId);
        }

        await notificationService.NotifyAsync(
            userId: domainEvent.UserId,
            type: domainEvent.ByAdmin
                ? EnumNotificationType.AccountForceLoggedOut
                : EnumNotificationType.SignedOutAllDevices,
            tokens: new Dictionary<string, string>(),
            cancellationToken: cancellationToken
        );
    }
}
