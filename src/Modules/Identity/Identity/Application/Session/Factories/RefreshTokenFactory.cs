using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Factories.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Configurations;

namespace _116.Identity.Application.Session.Factories;

/// <summary>
/// Factory implementation for handling refresh token validation and rotation logic.
/// Shared across public and admin refresh token use cases.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="refreshTokenService">Service for refresh token generation and hashing.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class RefreshTokenFactory(
    ISessionRepository sessionRepository,
    IRefreshTokenService refreshTokenService,
    IIdentityUnitOfWork unitOfWork
) : IRefreshTokenFactory
{
    /// <inheritdoc />
    public async Task<RefreshTokenData> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        string refreshTokenHash = refreshTokenService.HashRefreshToken(refreshToken: refreshToken);

        SessionEntity? session = await sessionRepository.GetByRefreshTokenHashAsync(
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

        session.UpdateRefreshToken(newRefreshTokenHash: newRefreshTokenHash, newExpiresAt: newRefreshTokenExpiresAt);

        return new RefreshTokenData(User: session.User, Session: session, NewRefreshToken: newRefreshToken);
    }

    /// <summary>
    /// Generates a new refresh token with its hash and expiration time.
    /// </summary>
    private (string token, string hash, DateTime expiresAt) GenerateNewRefreshToken()
    {
        var (_, _, _, _, refreshTokenExpirationMinutes) = AppEnvironment.Jwt();

        string newRefreshToken = refreshTokenService.GenerateRefreshToken();
        string newRefreshTokenHash = refreshTokenService.HashRefreshToken(refreshToken: newRefreshToken);
        DateTime newRefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(refreshTokenExpirationMinutes!));

        return (newRefreshToken, newRefreshTokenHash, newRefreshTokenExpiresAt);
    }
}
