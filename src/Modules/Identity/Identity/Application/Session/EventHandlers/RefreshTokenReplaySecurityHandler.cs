using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Session.EventHandlers;

/// <summary>
/// Reacts to a replayed refresh token: revokes the account's remaining sessions in its own scope
/// and commit, then alerts the owner by email. A replayed token means a deliberately invalidated
/// credential is circulating, so every session of the account is treated as suspect.
/// </summary>
/// <param name="sessionRepository">Repository revoking the user's sessions.</param>
/// <param name="unitOfWork">Unit of Work committing the revocation.</param>
/// <param name="userLookupService">Lookup resolving the recipient's name and address by id.</param>
/// <param name="mailer">Outbox mailer sending the security alert.</param>
/// <param name="logger">Logger recording skipped email deliveries.</param>
public class RefreshTokenReplaySecurityHandler(
    ISessionRepository sessionRepository,
    IIdentityUnitOfWork unitOfWork,
    IUserLookupService userLookupService,
    IMailer mailer,
    ILogger<RefreshTokenReplaySecurityHandler> logger
) : IDomainEventHandler<RefreshTokenReplayDetectedEvent>
{
    /// <inheritdoc />
    public async Task Handle(RefreshTokenReplayDetectedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await sessionRepository.DeleteAllByUserIdAsync(
            userId: domainEvent.UserId,
            reason: EnumSessionRevokeReason.SecurityInvalidation,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        AuthorInfo? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user?.Email is null)
        {
            logger.LogDebug(
                "Refresh token replay alert email skipped: user {UserId} has no email address.",
                domainEvent.UserId
            );
            return;
        }

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.RefreshTokenReplayAlert,
            to: new EmailRecipient(Address: user.Email, DisplayName: user.UserName),
            tokens: new Dictionary<string, string>
            {
                ["userName"] = user.UserName,
                ["time"] = DateTime.UtcNow.ToString("u"),
            },
            culture: EmailCulture.Current(),
            cancellationToken: cancellationToken
        );
    }
}
