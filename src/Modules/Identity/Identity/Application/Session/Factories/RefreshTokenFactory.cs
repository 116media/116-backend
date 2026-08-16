using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Factories.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Configurations;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Session.Factories;

/// <summary>
/// Factory implementation for handling refresh token validation and rotation logic.
/// Shared across public and admin refresh token use cases.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="refreshTokenService">Service for refresh token generation and hashing.</param>
/// <param name="tokenStateRepository">Repository providing the user's token-invalidation markers.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="sessionErrors">Session domain error factory for generating domain exceptions.</param>
/// <param name="logger">Logger recording replay detections that could not be completed.</param>
public class RefreshTokenFactory(
    ISessionRepository sessionRepository,
    IRefreshTokenService refreshTokenService,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    SessionErrors sessionErrors,
    ILogger<RefreshTokenFactory> logger
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

        if (session is not null)
        {
            if (!session.User.IsActive)
            {
                session.Revoke(reason: EnumSessionRevokeReason.SecurityInvalidation);
                await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
                throw sessionErrors.InvalidRefreshToken();
            }

            if (session.HasReachedAbsoluteExpiry())
            {
                session.Revoke(reason: EnumSessionRevokeReason.Expiry);
                await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
                throw sessionErrors.InvalidRefreshToken();
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

            UserSecurityState tokenState = await tokenStateRepository.GetOrCreateAsync(
                userId: session.UserId,
                cancellationToken: cancellationToken
            );

            return new RefreshTokenData(
                User: session.User,
                Session: session,
                NewRefreshToken: newRefreshToken,
                TokenState: tokenState
            );
        }

        // Replay detection is a reaction to the rejection, not part of deciding it: a failure to
        // record the replay must not turn the invalid-token rejection into a server error.
        try
        {
            await DetectReplayAsync(refreshTokenHash: refreshTokenHash, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Refresh token replay detection failed; the token is still rejected.");
        }

        throw sessionErrors.InvalidRefreshToken();
    }

    /// <summary>
    /// Checks whether the rejected refresh token belongs to an already-revoked session. A match
    /// means a deliberately invalidated credential is being presented again; the session records
    /// the replay and the commit publishes the fact so consumers can revoke the account's
    /// remaining sessions and alert the owner. The refresh attempt is rejected either way.
    /// </summary>
    /// <param name="refreshTokenHash">The hash of the rejected refresh token.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    private async Task DetectReplayAsync(string refreshTokenHash, CancellationToken cancellationToken)
    {
        SessionEntity? replayedSession = await sessionRepository.GetRevokedSessionByRefreshTokenHashAsync(
            refreshTokenHash: refreshTokenHash,
            cancellationToken: cancellationToken
        );

        if (replayedSession is null)
        {
            return;
        }

        replayedSession.RecordRefreshTokenReplay();
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
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
