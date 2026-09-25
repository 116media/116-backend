using _116.Identity.Application.Shared.OutboundEmails;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Sends the welcome email when a user's account transitions to verified. Reacting to the event
/// covers every verification path, so admin-driven verification produces the same welcome as the
/// public flow.
/// </summary>
/// <param name="userLookupService">Lookup resolving the recipient's name and address by id.</param>
/// <param name="messageDispatcher">Dispatcher routing the message to its recipients.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class UserVerifiedWelcomeEmailHandler(
    IUserLookupService userLookupService,
    IEmailDispatcher messageDispatcher,
    ILogger<UserVerifiedWelcomeEmailHandler> logger
) : IDomainEventHandler<UserVerifiedEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserVerifiedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorDto? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user?.Email is null)
        {
            logger.LogDebug("Welcome email skipped: user {UserId} has no email address.", domainEvent.UserId);
            return;
        }

        var message = new WelcomeEmail(
            User: new EmailRecipient(
                UserId: domainEvent.UserId,
                Address: user.Email,
                DisplayName: user.UserName,
                Locale: user.PreferredLocale
            )
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
    }
}
