using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions;

/// <summary>
/// Handles the <see cref="AdminCleanupExpiredSessionsCommand" /> to cleanup expired sessions.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="unitOfWork">Unit of work for transaction management.</param>
public class AdminCleanupExpiredSessionsHandler(
    ISessionRepository sessionRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminCleanupExpiredSessionsCommand, AdminCleanupExpiredSessionsResult>
{
    /// <summary>
    /// Handles the cleanup command by soft deleting all expired sessions.
    /// </summary>
    /// <param name="command">The command to cleanup expired sessions.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminCleanupExpiredSessionsResult" /> containing the number of deleted sessions.</returns>
    public async Task<AdminCleanupExpiredSessionsResult> Handle(
        AdminCleanupExpiredSessionsCommand command,
        CancellationToken cancellationToken
    )
    {
        int deletedCount = await sessionRepository.DeleteExpiredSessionsAsync(
            cancellationToken: cancellationToken
        );
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminCleanupExpiredSessionsResult(DeletedCount: deletedCount);
    }
}
