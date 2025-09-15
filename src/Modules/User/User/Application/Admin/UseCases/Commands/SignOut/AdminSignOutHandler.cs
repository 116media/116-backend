using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Repositories;
using _116.User.Domain.Entities;

namespace _116.User.Application.Admin.UseCases.Commands.SignOut;

/// <summary>
/// Handles the <see cref="AdminSignOutCommand"/> to sign out an admin user.
/// </summary>
public class AdminSignOutHandler(
    IUserRepository userRepository
) : ICommandHandler<AdminSignOutCommand, AdminSignOutResult>
{
    /// <summary>
    /// Handles the admin sign-out command by updating the user's login status.
    /// </summary>
    public async Task<AdminSignOutResult> Handle(AdminSignOutCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await userRepository.GetUserByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            return new AdminSignOutResult(IsSuccess: true);
        }

        userRepository.IsUserAccountActive(user);

        if (user.IsLoggedIn)
        {
            return new AdminSignOutResult(IsSuccess: true);
        }

        user.RecordLogout();
        await userRepository.UpdateAsync(user, cancellationToken);

        return new AdminSignOutResult(IsSuccess: true);
    }
}
