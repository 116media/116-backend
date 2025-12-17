using _116.Identity.Application.Shared.Persistence;
using _116.Shared.Contracts.Application.CQRS;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Public.UseCases.Commands.SignOut;

/// <summary>
/// Handles the <see cref="PublicSignOutCommand"/> to sign out a user.
/// </summary>
public class PublicSignOutHandler(
    IUserRepository userRepository,
    IAuthUnitOfWork unitOfWork
) : ICommandHandler<PublicSignOutCommand, PublicSignOutResult>
{
    /// <summary>
    /// Handles the sign-out command by updating the user's login status.
    /// </summary>
    public async Task<PublicSignOutResult> Handle(PublicSignOutCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await userRepository.FindUserByIdOrThrow(command.UserId, cancellationToken);

        // Validate user account status - user accounts must be active
        userRepository.IsUserAccountActive(user!);

        if (user!.IsLoggedIn == false)
        {
            return new PublicSignOutResult(IsSuccess: true);
        }

        user.RecordLogout();
        await unitOfWork.CommitAsync(cancellationToken);

        return new PublicSignOutResult(IsSuccess: true);
    }
}
