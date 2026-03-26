using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Session.Factories.Contracts;

/// <summary>
/// Contains refreshed session data with user and new refresh token.
/// </summary>
public record RefreshTokenData(UserEntity User, SessionEntity Session, string NewRefreshToken);

/// <summary>
/// Factory for handling refresh token validation and rotation logic.
/// Shared across public and admin refresh token use cases.
/// </summary>
public interface IRefreshTokenFactory
{
    /// <summary>
    /// Validates and rotates a refresh token, returning updated session data.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate and rotate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Auth data containing user, session, and new refresh token.</returns>
    Task<RefreshTokenData> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
