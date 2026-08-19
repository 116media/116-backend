using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.User.EventHandlers;

/// <summary>
/// Sends the security email and writes the in-app notification when a role is granted to a user.
/// The event payload carries the role name, so no user-role re-fetch happens after the commit.
/// Both channels are handled together because they share every lookup.
/// </summary>
/// <param name="userLookupService">Lookup resolving the recipient's name and address by id.</param>
/// <param name="mailer">Outbox mailer sending the role change notice.</param>
/// <param name="notifier">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped email deliveries.</param>
public class UserRoleGrantedNotificationsHandler(
    IUserLookupService userLookupService,
    IMailer mailer,
    INotifier notifier,
    ILogger<UserRoleGrantedNotificationsHandler> logger
) : IDomainEventHandler<UserRoleGrantedEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserRoleGrantedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorInfo? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user is null)
        {
            logger.LogDebug("Role grant notifications skipped: user {UserId} not found.", domainEvent.UserId);
            return;
        }

        string culture = EmailCulture.Current();

        if (user.Email is not null)
        {
            await mailer.EnqueueAsync(
                template: EnumEmailTemplate.RoleChanged,
                to: new EmailRecipient(Address: user.Email, DisplayName: user.UserName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = user.UserName,
                    ["roleName"] = domainEvent.RoleName,
                    ["action"] = "granted",
                },
                culture: culture,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            logger.LogDebug("Role grant email skipped: user {UserId} has no email address.", domainEvent.UserId);
        }

        await notifier.NotifyAsync(
            userId: domainEvent.UserId,
            type: EnumNotificationType.RoleChanged,
            tokens: new Dictionary<string, string> { ["roleName"] = domainEvent.RoleName, ["action"] = "granted" },
            culture: culture,
            cancellationToken: cancellationToken
        );
    }
}
