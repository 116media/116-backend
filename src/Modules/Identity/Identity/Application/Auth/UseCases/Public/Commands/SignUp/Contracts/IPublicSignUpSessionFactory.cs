using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.Contracts;

/// <summary>
/// Contains authentication session data including tokens and expiration times.
/// </summary>
public record PublicSignUpSessionData(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt
);

/// <summary>
/// Factory for creating authentication sessions with tokens and metadata.
/// </summary>
public interface IPublicSignUpSessionFactory
{
    /// <summary>
    /// Creates a new authentication session for a user.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="userPermissions">List of user permissions for JWT claims.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Session data containing access token, refresh token, and expiration times.</returns>
    Task<PublicSignUpSessionData> CreateSessionAsync(
        UserEntity user,
        List<RolePermissionEntity> userPermissions,
        CancellationToken cancellationToken
    );
}
