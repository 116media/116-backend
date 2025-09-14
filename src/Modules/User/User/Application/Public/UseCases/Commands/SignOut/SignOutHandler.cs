using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Repositories;
using _116.User.Domain.Entities;

namespace _116.User.Application.Public.UseCases.Commands.SignOut;

/// <summary>
/// Handles the <see cref="SignOutCommand"/> to sign out a user.
/// </summary>
public class SignOutHandler(
    IUserRepository userRepository
) : ICommandHandler<SignOutCommand, SignOutResult>
{
    /// <summary>
    /// Handles the sign-out command by updating the user's login status.
    /// </summary>
    public async Task<SignOutResult> Handle(SignOutCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await userRepository.GetUserByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            return new SignOutResult(IsSuccess: true);
        }

        userRepository.IsUserAccountActive(user);

        if (user.IsLoggedIn)
        {
            return new SignOutResult(IsSuccess: true);
        }

        user.RecordLogout();
        await userRepository.UpdateAsync(user, cancellationToken);

        return new SignOutResult(IsSuccess: true);
    }
}
