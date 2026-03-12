using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices;

/// <summary>
/// Handles the <see cref="PublicSignOutFromAllDevicesCommand" /> to sign out a user from all devices.
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
        UserEntity? user = await authRepository.FindUserByIdOrThrow(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        // Validate user account status
        authRepository.IsUserAccountActive(user!);

        await sessionRepository.DeleteAllByUserIdAsync(userId: user!.Id, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicSignOutFromAllDevicesResult(IsSuccess: true);
    }
}
