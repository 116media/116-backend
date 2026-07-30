using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RevokeSession;

/// <summary>
/// Handles the <see cref="PublicRevokeSessionCommand" /> to revoke a specific user session.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="unitOfWork">Unit of work for transaction management.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class PublicRevokeSessionHandler(
    ISessionRepository sessionRepository,
    IIdentityUnitOfWork unitOfWork,
    IdentityI18n i18n
) : ICommandHandler<PublicRevokeSessionCommand, PublicRevokeSessionResult>
{
    /// <summary>
    /// Handles the revoke session command by soft deleting the specified session.
    /// </summary>
    /// <param name="command">The command containing user ID and session ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicRevokeSessionResult" /> indicating success.</returns>
    /// <exception cref="NotFoundException">Thrown when the session is not found.</exception>
    /// <exception cref="AuthorizationException">Thrown when the session doesn't belong to the user.</exception>
    public async Task<PublicRevokeSessionResult> Handle(
        PublicRevokeSessionCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid sessionId = Guid.Parse(command.SessionId);

        SessionEntity? session = await sessionRepository.GetByIdAsync(
            sessionId: sessionId,
            cancellationToken: cancellationToken
        );

        // Verify the session belongs to the user
        if (session is null || session.UserId != command.UserId)
        {
            throw i18n.Session.SessionNotFound(sessionId: sessionId);
        }

        await sessionRepository.RevokeAsync(
            sessionId: sessionId,
            reason: EnumSessionRevokeReason.SelfSignOut,
            cancellationToken: cancellationToken
        );
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicRevokeSessionResult(IsSuccess: true);
    }
}
