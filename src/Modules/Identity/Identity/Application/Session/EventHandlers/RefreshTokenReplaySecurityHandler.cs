using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Messages;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.Messages;
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
/// <param name="messageDispatcher">Dispatcher routing the security alert to its recipients.</param>
/// <param name="logger">Logger recording skipped email deliveries.</param>
public class RefreshTokenReplaySecurityHandler(
    ISessionRepository sessionRepository,
    IIdentityUnitOfWork unitOfWork,
    IUserLookupService userLookupService,
    IMessageDispatcher messageDispatcher,
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

        AuthorDto? user = await userLookupService.GetAuthorInfoByIdAsync(
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

        var message = new RefreshTokenReplayMessage(
            User: new MessageRecipient(
                UserId: domainEvent.UserId,
                Address: user.Email,
                DisplayName: user.UserName,
                Locale: user.PreferredLocale
            ),
            DetectedAt: DateTimeOffset.UtcNow
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
    }
}
