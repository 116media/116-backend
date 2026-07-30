using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
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
/// <param name="mailer">Outbox mailer sending the security notice.</param>
/// <param name="notifier">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped email deliveries.</param>
public class UserSignedOutAllDevicesNotificationsHandler(
    IUserLookupService userLookupService,
    IMailer mailer,
    INotifier notifier,
    ILogger<UserSignedOutAllDevicesNotificationsHandler> logger
) : IDomainEventHandler<UserSignedOutAllDevicesEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserSignedOutAllDevicesEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorInfo? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user is null)
        {
            logger.LogDebug("Mass sign-out notifications skipped: user {UserId} not found.", domainEvent.UserId);
            return;
        }

        string culture = EmailCulture.Current();

        if (user.Email is not null)
        {
            await mailer.EnqueueAsync(
                template: domainEvent.ByAdmin
                    ? EnumEmailTemplate.AccountForceLoggedOut
                    : EnumEmailTemplate.SignedOutAllDevices,
                to: new EmailRecipient(Address: user.Email, DisplayName: user.UserName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = user.UserName,
                    ["time"] = DateTime.UtcNow.ToString("u"),
                },
                culture: culture,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            logger.LogDebug("Mass sign-out email skipped: user {UserId} has no email address.", domainEvent.UserId);
        }

        await notifier.NotifyAsync(
            userId: domainEvent.UserId,
            type: domainEvent.ByAdmin
                ? EnumNotificationType.AccountForceLoggedOut
                : EnumNotificationType.SignedOutAllDevices,
            tokens: new Dictionary<string, string>(),
            culture: culture,
            cancellationToken: cancellationToken
        );
    }
}
