using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Session.Factories.Contracts;

/// <summary>
/// Result of session creation containing tokens and expiration times.
/// </summary>
public record SessionResult(
    string RefreshToken,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt
);

/// <summary>
/// Factory for creating authentication sessions with tokens and metadata.
/// Centralizes the common logic for session creation across login, signup, and social login flows.
/// </summary>
public interface ISessionFactory
{
    /// <summary>
    /// Creates a new authentication session for a user or reuses an existing active session.
    /// If an active session already exists for the same device, it will be reused with rotated tokens.
    /// </summary>
    /// <param name="user">The user entity to create session for.</param>
    /// <param name="userPermissions">The user's role permissions.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Session creation result containing tokens and expiration times.</returns>
    Task<SessionResult> CreateSessionAsync(
        UserEntity user,
        List<RolePermissionEntity> userPermissions,
        CancellationToken cancellationToken
    );
}
