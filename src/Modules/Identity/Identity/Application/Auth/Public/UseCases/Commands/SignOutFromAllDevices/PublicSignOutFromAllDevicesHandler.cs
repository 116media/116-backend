using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.SignOutFromAllDevices;

/// <summary>
/// Handles the <see cref="PublicSignOutFromAllDevicesCommand"/> to sign out a user from all devices.
/// </summary>
public class PublicSignOutFromAllDevicesHandler(
    IAuthRepository authRepository,
    ISessionRepository sessionRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<PublicSignOutFromAllDevicesCommand, PublicSignOutFromAllDevicesResult>
{
    /// <summary>
    /// Handles the sign-out command by invalidating all user sessions across all devices.
    /// </summary>
    public async Task<PublicSignOutFromAllDevicesResult> Handle(
        PublicSignOutFromAllDevicesCommand command,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.FindUserByIdOrThrow(command.UserId, cancellationToken);
        // Validate user account status
        authRepository.IsUserAccountActive(user!);

        await sessionRepository.DeleteAllByUserIdAsync(user!.Id, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new PublicSignOutFromAllDevicesResult(IsSuccess: true);
    }
}
