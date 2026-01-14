namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Service for refresh token generation and hashing operations.
/// Handles secure token creation and storage for session management.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// </summary>
    /// <returns>A base64-encoded refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Hashes a refresh token for secure storage in the database.
    /// </summary>
    /// <param name="refreshToken">The plain refresh token to hash</param>
    /// <returns>The hashed refresh token string</returns>
    string HashRefreshToken(string refreshToken);
}
