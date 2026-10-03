using _116.Identity.Application.Shared.Cache;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Session.Services;

/// <summary>
/// Contains refreshed session data with user, new refresh token and the user's current
/// token-invalidation markers.
/// </summary>
public record RefreshTokenData(
    UserEntity User,
    SessionEntity Session,
    string NewRefreshToken,
    UserSecurityState TokenState
);

/// <summary>
/// Service for handling refresh token validation and rotation logic.
/// Shared across public and admin refresh token use cases.
/// </summary>
public interface IRefreshTokenRotationService
{
    /// <summary>
    /// Validates and rotates a refresh token, returning updated session data.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate and rotate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Auth data containing user, session, and new refresh token.</returns>
    Task<RefreshTokenData> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
