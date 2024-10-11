using _116.Shared.Application.Persistence;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Domain.Entities;

namespace _116.Auth.Application.Admin.UseCases.Commands.SignOut;

/// <summary>
/// Handles the <see cref="AdminSignOutCommand"/> to sign out an admin user.
/// </summary>
public class AdminSignOutHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<AdminSignOutCommand, AdminSignOutResult>
{
    /// <summary>
    /// Handles the admin sign-out command by updating the user's login status.
    /// </summary>
    public async Task<AdminSignOutResult> Handle(AdminSignOutCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await userRepository.FindUserByIdOrThrow(command.UserId, cancellationToken);

        // Validate user account status - admin accounts must be active
        userRepository.IsUserAccountActive(user!);

        if (user!.IsLoggedIn == false)
        {
            return new AdminSignOutResult(IsSuccess: true);
        }

        user.RecordLogout();
        await unitOfWork.CommitAsync(cancellationToken);

        return new AdminSignOutResult(IsSuccess: true);
    }
}
