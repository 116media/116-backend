using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;

/// <summary>
/// Factory implementation for handling user sign-out session management.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="refreshTokenService">Service for refresh token hashing.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSignOutSessionFactory(
    ISessionRepository sessionRepository,
    IRefreshTokenService refreshTokenService,
    IIdentityUnitOfWork unitOfWork
) : IPublicSignOutSessionFactory
{
    /// <summary>
    /// Signs out a user by invalidating their session associated with the refresh token.
    /// </summary>
    public async Task SignOutAsync(
        string refreshToken,
        CancellationToken cancellationToken
    )
    {
        string refreshTokenHash = refreshTokenService.HashRefreshToken(refreshToken: refreshToken);
        SessionEntity? session = await sessionRepository.GetByRefreshTokenHashAsync(
            refreshTokenHash: refreshTokenHash,
            cancellationToken: cancellationToken
        );

        if (session != null)
        {
            await sessionRepository.DeleteAsync(sessionId: session.Id, cancellationToken: cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }
    }
}
