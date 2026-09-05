using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Shared.Application.Localization;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Sends the security email and writes the in-app notification when a user's password changes.
/// The change origin selects both the email template and the notification type; both channels are
/// handled together because they share every lookup.
/// </summary>
/// <param name="userLookupService">Lookup resolving the recipient's name and address by id.</param>
/// <param name="mailer">Outbox mailer sending the security confirmation.</param>
/// <param name="notifier">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped email deliveries.</param>
public class UserPasswordChangedNotificationsHandler(
    IUserLookupService userLookupService,
    IMailer mailer,
    INotifier notifier,
    ILogger<UserPasswordChangedNotificationsHandler> logger
) : IDomainEventHandler<UserPasswordChangedEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserPasswordChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorInfo? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user is null)
        {
            logger.LogDebug("Password change notifications skipped: user {UserId} not found.", domainEvent.UserId);
            return;
        }

        string culture = EmailCulture.Current();

        if (user.Email is not null)
        {
            await mailer.EnqueueAsync(
                template: EmailTemplateFor(origin: domainEvent.Origin),
                to: new EmailRecipient(Address: user.Email, DisplayName: user.UserName),
                tokens: EmailTokensFor(origin: domainEvent.Origin, userName: user.UserName),
                culture: culture,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            logger.LogDebug("Password change email skipped: user {UserId} has no email address.", domainEvent.UserId);
        }

        await notifier.NotifyAsync(
            userId: domainEvent.UserId,
            type: NotificationTypeFor(origin: domainEvent.Origin),
            tokens: new Dictionary<string, string>(),
            culture: culture,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Maps the change origin to the email template shipped for that flow.
    /// </summary>
    /// <param name="origin">The flow that replaced the password.</param>
    /// <returns>The template to render.</returns>
    private static EnumEmailTemplate EmailTemplateFor(EnumPasswordChangeOrigin origin)
    {
        return origin switch
        {
            EnumPasswordChangeOrigin.Reset => EnumEmailTemplate.PasswordResetCompleted,
            EnumPasswordChangeOrigin.SetLocal => EnumEmailTemplate.LocalPasswordAdded,
            _ => EnumEmailTemplate.PasswordChanged,
        };
    }

    /// <summary>
    /// Builds the token set the origin's template requires. The change and reset templates carry
    /// a timestamp under their historical token names; the set-local template needs only the name.
    /// </summary>
    /// <param name="origin">The flow that replaced the password.</param>
    /// <param name="userName">The recipient's display name.</param>
    /// <returns>The tokens for the template render.</returns>
    private static Dictionary<string, string> EmailTokensFor(EnumPasswordChangeOrigin origin, string userName)
    {
        return origin switch
        {
            EnumPasswordChangeOrigin.Reset => new Dictionary<string, string>
            {
                ["userName"] = userName,
                ["resetTime"] = DateTime.UtcNow.ToString("u"),
            },
            EnumPasswordChangeOrigin.SetLocal => new Dictionary<string, string> { ["userName"] = userName },
            _ => new Dictionary<string, string>
            {
                ["userName"] = userName,
                ["changeTime"] = DateTime.UtcNow.ToString("u"),
            },
        };
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
