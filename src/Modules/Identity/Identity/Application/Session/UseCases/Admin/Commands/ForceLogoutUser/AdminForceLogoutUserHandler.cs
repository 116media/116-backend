using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Mailer.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;

/// <summary>
/// Handles the <see cref="AdminForceLogoutUserCommand" /> to force-logout a user from all sessions.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="unitOfWork">Unit of work for transaction management.</param>
/// <param name="authRepository">Repository resolving the notified user.</param>
/// <param name="mailer">Outbox mailer notifying the ejected user.</param>
public class AdminForceLogoutUserHandler(
    ISessionRepository sessionRepository,
    IIdentityUnitOfWork unitOfWork,
    IAuthRepository authRepository,
    IMailer mailer
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

        await sessionRepository.DeleteAllByUserIdAsync(userId: targetUserId, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        // Force-logout is an idempotent no-op for unknown user ids (the
        // endpoint contract answers 200 either way), so a missing user only
        // skips the notification instead of failing the command.
        UserEntity? user;

        try
        {
            user = await authRepository.FindUserByIdOrThrow(userId: targetUserId, cancellationToken: cancellationToken);
        }
        catch (NotFoundException)
        {
            return new AdminForceLogoutUserResult(IsSuccess: true);
        }

        if (user?.Email is not null)
        {
            await mailer.EnqueueAsync(
                template: EnumEmailTemplate.AccountForceLoggedOut,
                to: new EmailRecipient(Address: user!.Email, DisplayName: user.UserName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = user.UserName,
                    ["time"] = DateTime.UtcNow.ToString("u"),
                },
                culture: EmailCulture.Current(),
                cancellationToken: cancellationToken
            );
        }

        return new AdminForceLogoutUserResult(IsSuccess: true);
    }
}
