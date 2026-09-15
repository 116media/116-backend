using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.ActivateUser;

/// <summary>
/// Handles the <see cref="AdminActivateUserCommand" /> to reactivate a user account so the
/// user can log in again. Sessions and tokens are untouched: none survived the deactivation.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminActivateUserHandler(IAuthRepository authRepository, IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<AdminActivateUserCommand, AdminActivateUserResult>
{
    /// <summary>
    /// Handles the activate user command. Idempotent: activating an already-active account
    /// succeeds without raising a second event.
    /// </summary>
    /// <param name="command">The command containing the user ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminActivateUserResult" /> reporting the account state.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    public async Task<AdminActivateUserResult> Handle(
        AdminActivateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid userId = Guid.Parse(input: command.UserId);

        UserEntity? user = await authRepository.FindUserByIdOrThrow(
            userId: userId,
            cancellationToken: cancellationToken
        );

        user!.Activate();
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminActivateUserResult(IsSuccess: true);
    }
}
