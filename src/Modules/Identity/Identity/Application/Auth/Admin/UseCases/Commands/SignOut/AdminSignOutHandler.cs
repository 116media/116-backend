using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.SignOut;

/// <summary>
/// Handles the <see cref="AdminSignOutCommand"/> to sign out an admin user.
/// </summary>
public class AdminSignOutHandler(
    IUserRepository userRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminSignOutCommand, AdminSignOutResult>
{
    /// <summary>
    /// Handles the admin sign-out command by invalidating user sessions.
    /// </summary>
    public async Task<AdminSignOutResult> Handle(AdminSignOutCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await userRepository.FindUserByIdOrThrow(command.UserId, cancellationToken);
        // Validate user account status
        userRepository.IsUserAccountActive(user!);
        // TODO: Delete SessionEntity records when implementing session management
        // For now, sign out only invalidates the client-side token
        await unitOfWork.CommitAsync(cancellationToken);
        return new AdminSignOutResult(IsSuccess: true);
    }
}
