using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Services;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.SignOut;

/// <summary>
/// Handles the <see cref="AdminSignOutCommand"/> to sign out an admin user from current device.
/// </summary>
public class AdminSignOutHandler(
    IAuthRepository authRepository,
    ISessionRepository sessionRepository,
    IRefreshTokenService refreshTokenService,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminSignOutCommand, AdminSignOutResult>
{
    /// <summary>
    /// Handles the admin sign-out command by invalidating the specific session.
    /// This operation is idempotent - if the session doesn't exist, user is already logged out.
    /// </summary>
    public async Task<AdminSignOutResult> Handle(AdminSignOutCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await authRepository.FindUserByIdOrThrow(command.UserId, cancellationToken);
        // Validate user account status
        authRepository.IsUserAccountActive(user!);

        string refreshTokenHash = refreshTokenService.HashRefreshToken(command.RefreshToken);
        SessionEntity? session = await sessionRepository.GetByRefreshTokenHashAsync(
            refreshTokenHash,
            cancellationToken
        );

        if (session != null)
        {
            await sessionRepository.DeleteAsync(session.Id, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        // Always return success - logout is idempotent
        // If session doesn't exist, user is effectively already logged out
        return new AdminSignOutResult(IsSuccess: true);
    }
}
