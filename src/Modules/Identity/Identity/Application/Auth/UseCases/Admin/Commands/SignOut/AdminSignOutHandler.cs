using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut;

/// <summary>
/// Handles the <see cref="AdminSignOutCommand" /> to sign out an admin user from current device.
/// </summary>
/// <param name="sessionFactory">Factory for handling admin user sign-out session management.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
public class AdminSignOutHandler(IAdminSignOutSessionFactory sessionFactory, IAuthRepository authRepository)
    : ICommandHandler<AdminSignOutCommand, AdminSignOutResult>
{
    /// <summary>
    /// Handles the admin sign-out command by invalidating the specific session.
    /// This operation is idempotent - if the session doesn't exist, user is already logged out.
    /// </summary>
    public async Task<AdminSignOutResult> Handle(AdminSignOutCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await authRepository.FindUserByIdOrThrow(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);

        await sessionFactory.SignOutAsync(refreshToken: command.RefreshToken, cancellationToken: cancellationToken);

        return new AdminSignOutResult(IsSuccess: true);
    }
}
