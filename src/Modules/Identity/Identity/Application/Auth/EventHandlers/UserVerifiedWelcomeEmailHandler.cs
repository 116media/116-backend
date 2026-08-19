using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Sends the welcome email when a user's account transitions to verified. Reacting to the event
/// covers every verification path, so admin-driven verification produces the same welcome as the
/// public flow.
/// </summary>
/// <param name="userLookupService">Lookup resolving the recipient's name and address by id.</param>
/// <param name="mailer">Outbox mailer sending the welcome email.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class UserVerifiedWelcomeEmailHandler(
    IUserLookupService userLookupService,
    IMailer mailer,
    ILogger<UserVerifiedWelcomeEmailHandler> logger
) : IDomainEventHandler<UserVerifiedEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserVerifiedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorInfo? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user?.Email is null)
        {
            logger.LogDebug("Welcome email skipped: user {UserId} has no email address.", domainEvent.UserId);
            return;
        }

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.Welcome,
            to: new EmailRecipient(Address: user.Email, DisplayName: user.UserName),
            tokens: new Dictionary<string, string> { ["userName"] = user.UserName },
            culture: EmailCulture.Current(),
            cancellationToken: cancellationToken
        );
    }
}
