using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;

/// <summary>
/// Handles the <see cref="AdminForceLogoutUserCommand" /> to force-logout a user from all sessions.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="unitOfWork">Unit of work for transaction management.</param>
public class AdminForceLogoutUserHandler(ISessionRepository sessionRepository, IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<AdminForceLogoutUserCommand, AdminForceLogoutUserResult>
{
    /// <summary>
    /// Handles the force logout command by deleting all sessions for the target user.
    /// </summary>
    /// <param name="command">The command containing the target user ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminForceLogoutUserResult" /> indicating success.</returns>
    public async Task<AdminForceLogoutUserResult> Handle(
        AdminForceLogoutUserCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid targetUserId = Guid.Parse(input: command.UserId);

        await sessionRepository.DeleteAllByUserIdAsync(userId: targetUserId, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminForceLogoutUserResult(true);
    }
}
