using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.Contracts;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Configurations;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;

/// <summary>
/// Factory implementation for handling refresh token validation and rotation logic.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="refreshTokenService">Service for refresh token generation and hashing.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicRefreshTokenFactory(
    ISessionRepository sessionRepository,
    IRefreshTokenService refreshTokenService,
    IRoleRepository roleRepository,
    IIdentityUnitOfWork unitOfWork
) : IPublicRefreshTokenFactory
{
    /// <summary>
    /// Validates and rotates a refresh token, returning updated session data.
    /// </summary>
    public async Task<PublicRefreshTokenData> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken
    )
    {
        string refreshTokenHash = refreshTokenService.HashRefreshToken(refreshToken: refreshToken);

        SessionEntity? session =
            await sessionRepository.GetByRefreshTokenHashAsync(
                refreshTokenHash: refreshTokenHash,
                cancellationToken: cancellationToken
            );

        if (session is null)
        {
            throw SessionErrors.InvalidRefreshToken();
        }

        var (newRefreshToken, newRefreshTokenHash, newRefreshTokenExpiresAt) = GenerateNewRefreshToken();

        await sessionRepository.UpdateRefreshTokenAsync(
            sessionId: session.Id,
            newRefreshTokenHash: newRefreshTokenHash,
            newExpiresAt: newRefreshTokenExpiresAt,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        session.UpdateRefreshToken(
            newRefreshTokenHash: newRefreshTokenHash,
            newExpiresAt: newRefreshTokenExpiresAt
        );

        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: session.User.UserRoles);

        return new PublicRefreshTokenData(
            Roles: roles,
            Session: session,
            User: session.User,
            Permissions: permissions,
            NewRefreshToken: newRefreshToken
        );
    }

    /// <summary>
    /// Generates a new refresh token with its hash and expiration time.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// - token: The plain refresh token to return to the client
    /// - hash: The hashed refresh token for secure storage
    /// - expiresAt: The expiration timestamp for the refresh token
    /// </returns>
    private (string token, string hash, DateTime expiresAt) GenerateNewRefreshToken()
    {
        var (_, _, _, _, refreshTokenExpirationMinutes) = AppEnvironment.Jwt();

        string newRefreshToken = refreshTokenService.GenerateRefreshToken();
        string newRefreshTokenHash = refreshTokenService.HashRefreshToken(refreshToken: newRefreshToken);
        DateTime newRefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(refreshTokenExpirationMinutes!));

        return (newRefreshToken, newRefreshTokenHash, newRefreshTokenExpiresAt);
    }
}
