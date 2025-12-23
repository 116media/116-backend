using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.SignOutFromAllDevices;

/// <summary>
/// Handles the <see cref="AdminSignOutFromAllDevicesCommand"/> to sign out an admin user from all devices.
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
        UserEntity? user = await authRepository.FindUserByIdOrThrow(command.UserId, cancellationToken);
        // Validate user account status
        authRepository.IsUserAccountActive(user!);

        // Delete all active sessions for the user (soft delete)
        await sessionRepository.DeleteAllByUserIdAsync(user!.Id, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new AdminSignOutFromAllDevicesResult(IsSuccess: true);
    }
}
