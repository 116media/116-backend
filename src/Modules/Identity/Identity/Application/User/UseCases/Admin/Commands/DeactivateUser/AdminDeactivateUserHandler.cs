using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.DeactivateUser;

/// <summary>
/// Handles the <see cref="AdminDeactivateUserCommand" /> to deactivate a user account. Session
/// revocation and the token-version bump react to the deactivation event the user aggregate
/// raises.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminDeactivateUserHandler(IAuthRepository authRepository, IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<AdminDeactivateUserCommand, AdminDeactivateUserResult>
{
    /// <summary>
    /// Handles the deactivate user command. Idempotent: deactivating an already-inactive
    /// account succeeds without raising a second event.
    /// </summary>
    /// <param name="command">The command containing the user ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminDeactivateUserResult" /> reporting the account state.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    public async Task<AdminDeactivateUserResult> Handle(
        AdminDeactivateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid userId = Guid.Parse(input: command.UserId);

        UserEntity? user = await authRepository.FindUserByIdOrThrow(
            userId: userId,
            cancellationToken: cancellationToken
        );

        user!.Deactivate();
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeactivateUserResult(IsSuccess: true);
    }
}
