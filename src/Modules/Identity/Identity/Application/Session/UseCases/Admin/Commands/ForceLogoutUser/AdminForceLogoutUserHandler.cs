using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;

/// <summary>
/// Handles the <see cref="AdminForceLogoutUserCommand" /> to force-logout a user from all sessions.
/// The security email and in-app notification react to the domain event the user aggregate raises
/// for the admin-driven mass sign-out.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="unitOfWork">Unit of work for transaction management.</param>
/// <param name="authRepository">Repository resolving the target user.</param>
public class AdminForceLogoutUserHandler(
    ISessionRepository sessionRepository,
    IIdentityUnitOfWork unitOfWork,
    IAuthRepository authRepository
) : ICommandHandler<AdminForceLogoutUserCommand, AdminForceLogoutUserResult>
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

        // Force-logout is an idempotent no-op for unknown user ids (the
        // endpoint contract answers 200 either way), so a missing user only
        // skips the mass sign-out fact instead of failing the command.
        UserEntity? user;

        try
        {
            user = await authRepository.FindUserByIdOrThrow(userId: targetUserId, cancellationToken: cancellationToken);
        }
        catch (NotFoundException)
        {
            user = null;
        }

        user?.RecordMassSignOut(byAdmin: true);
        await sessionRepository.DeleteAllByUserIdAsync(
            userId: targetUserId,
            reason: EnumSessionRevokeReason.AdminRevoke,
            cancellationToken: cancellationToken
        );
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminForceLogoutUserResult(IsSuccess: true);
    }
}
