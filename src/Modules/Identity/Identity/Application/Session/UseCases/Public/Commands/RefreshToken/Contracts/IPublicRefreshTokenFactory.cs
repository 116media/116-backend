using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.Contracts;

/// <summary>
/// Contains refreshed session data with user and new refresh token.
/// </summary>
public record PublicRefreshTokenData(UserEntity User, SessionEntity Session, string NewRefreshToken);

/// <summary>
/// Factory for handling refresh token validation and rotation logic.
/// </summary>
public interface IPublicRefreshTokenFactory
{
    /// <summary>
    /// Validates and rotates a refresh token, returning updated session data.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate and rotate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Auth data containing user, session, roles, and permissions.</returns>
    Task<PublicRefreshTokenData> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
