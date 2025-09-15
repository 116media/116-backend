using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Repositories;
using _116.User.Domain.Entities;

namespace _116.User.Application.Public.UseCases.Commands.SignOut;

/// <summary>
/// Handles the <see cref="PublicSignOutCommand"/> to sign out a user.
/// </summary>
public class PublicSignOutHandler(
    IUserRepository userRepository
) : ICommandHandler<PublicSignOutCommand, PublicSignOutResult>
{
    /// <summary>
    /// Handles the sign-out command by updating the user's login status.
    /// </summary>
    public async Task<PublicSignOutResult> Handle(PublicSignOutCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await userRepository.GetUserByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            return new PublicSignOutResult(IsSuccess: true);
        }

        userRepository.IsUserAccountActive(user);

        if (user.IsLoggedIn)
        {
            return new PublicSignOutResult(IsSuccess: true);
        }

        user.RecordLogout();
        await userRepository.UpdateAsync(user, cancellationToken);

        return new PublicSignOutResult(IsSuccess: true);
    }
}
