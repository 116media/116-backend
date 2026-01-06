using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;

/// <summary>
/// Handles the <see cref="PublicSignOutCommand" /> to sign out a user from current device.
/// </summary>
/// <param name="sessionFactory">Factory for handling user sign-out session management.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
public class PublicSignOutHandler(IPublicSignOutSessionFactory sessionFactory, IAuthRepository authRepository)
    : ICommandHandler<PublicSignOutCommand, PublicSignOutResult>
{
    /// <summary>
    /// Handles the sign-out command by invalidating the specific session.
    /// This operation is idempotent - if the session doesn't exist, user is already logged out.
    /// </summary>
    public async Task<PublicSignOutResult> Handle(PublicSignOutCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await authRepository.FindUserByIdOrThrow(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);

        await sessionFactory.SignOutAsync(refreshToken: command.RefreshToken, cancellationToken: cancellationToken);

        return new PublicSignOutResult(true);
    }
}
