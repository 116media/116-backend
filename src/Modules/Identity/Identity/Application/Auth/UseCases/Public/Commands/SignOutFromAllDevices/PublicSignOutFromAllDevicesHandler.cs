using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices;

/// <summary>
/// Handles the <see cref="PublicSignOutFromAllDevicesCommand" /> to sign out a user from all devices.
/// The security email and in-app notification react to the domain event the user aggregate raises
/// for the mass sign-out.
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

        user!.RecordMassSignOut(byAdmin: false);
        await sessionRepository.DeleteAllByUserIdAsync(
            userId: user.Id,
            reason: EnumSessionRevokeReason.SelfSignOut,
            cancellationToken: cancellationToken
        );
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicSignOutFromAllDevicesResult(IsSuccess: true);
    }
}
