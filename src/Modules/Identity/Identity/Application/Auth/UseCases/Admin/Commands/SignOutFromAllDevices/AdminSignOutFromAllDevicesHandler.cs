using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOutFromAllDevices;

/// <summary>
/// Handles the <see cref="AdminSignOutFromAllDevicesCommand" /> to sign out an admin user from all devices.
/// </summary>
public class AdminSignOutFromAllDevicesHandler(
    IAuthRepository authRepository,
    ISessionRepository sessionRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminSignOutFromAllDevicesCommand, AdminSignOutFromAllDevicesResult>
{
    /// <summary>
    /// Handles the admin sign-out command by invalidating all user sessions across all devices.
    /// </summary>
    public async Task<AdminSignOutFromAllDevicesResult> Handle(
        AdminSignOutFromAllDevicesCommand command,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.FindUserByIdOrThrow(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );
        // Validate user account status
        authRepository.IsUserAccountActive(user!);

        // Delete all active sessions for the user (soft delete)
        await sessionRepository.DeleteAllByUserIdAsync(userId: user!.Id, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminSignOutFromAllDevicesResult(true);
    }
}
