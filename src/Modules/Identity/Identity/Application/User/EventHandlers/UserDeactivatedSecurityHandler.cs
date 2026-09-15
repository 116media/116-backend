using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Shared.Application.Services;

namespace _116.Identity.Application.User.EventHandlers;

/// <summary>
/// Reacts to an account deactivation by revoking every live session and bumping the token
/// version, so a deactivated user is signed out everywhere and live JWTs die on refresh.
/// Each revocation lands in the audit trail through <see cref="SessionRevokedEvent" />, so
/// nothing is logged here.
/// </summary>
/// <param name="sessionRepository">Repository revoking the user's sessions.</param>
/// <param name="tokenStateRepository">Repository bumping the user's token version.</param>
/// <param name="unitOfWork">Unit of Work committing the revocation.</param>
public class UserDeactivatedSecurityHandler(
    ISessionRepository sessionRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork
) : IDomainEventHandler<UserDeactivatedEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserDeactivatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await sessionRepository.DeleteAllByUserIdAsync(
            userId: domainEvent.UserId,
            reason: EnumSessionRevokeReason.SecurityInvalidation,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        await tokenStateRepository.BumpTokenVersionAsync(
            userId: domainEvent.UserId,
            cancellationToken: cancellationToken
        );
    }
}
