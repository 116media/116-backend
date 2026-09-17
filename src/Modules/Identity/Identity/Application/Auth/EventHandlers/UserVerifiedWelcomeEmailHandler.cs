using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.DTOs;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Shared.Application.Localization;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Sends the welcome email when a user's account transitions to verified. Reacting to the event
/// covers every verification path, so admin-driven verification produces the same welcome as the
/// public flow.
/// </summary>
/// <param name="userLookupService">Lookup resolving the recipient's name and address by id.</param>
/// <param name="emailService">Outbox mailer sending the welcome email.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class UserVerifiedWelcomeEmailHandler(
    IUserLookupService userLookupService,
    IEmailService emailService,
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

        await emailService.EnqueueAsync(
            template: EnumEmailTemplate.Welcome,
            to: new EmailRecipientDto(Address: user.Email, DisplayName: user.UserName),
            tokens: new Dictionary<string, string> { ["userName"] = user.UserName },
            culture: EmailCulture.Current(),
            cancellationToken: cancellationToken
        );
    }
}
