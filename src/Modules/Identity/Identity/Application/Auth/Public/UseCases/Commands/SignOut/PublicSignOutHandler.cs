using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.SignOut;

/// <summary>
/// Handles the <see cref="PublicSignOutCommand"/> to sign out a user.
/// </summary>
public class PublicSignOutHandler(
    IUserRepository userRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<PublicSignOutCommand, PublicSignOutResult>
{
    /// <summary>
    /// Handles the sign-out command by invalidating user sessions.
    /// </summary>
    public async Task<PublicSignOutResult> Handle(PublicSignOutCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await userRepository.FindUserByIdOrThrow(command.UserId, cancellationToken);
        // Validate user account status
        userRepository.IsUserAccountActive(user!);
        // TODO: Delete SessionEntity records when implementing session management
        // For now, sign out only invalidates the client-side token
        await unitOfWork.CommitAsync(cancellationToken);
        return new PublicSignOutResult(IsSuccess: true);
    }
}
