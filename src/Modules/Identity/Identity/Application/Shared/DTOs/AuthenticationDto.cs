namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// Carries a successful authentication between the application's pieces: the user, the issued
/// tokens and their expiry. Handlers produce it, the token delivery service reads it, and
/// endpoints project it onto their own response shapes.
/// </summary>
/// <param name="User">Complete user information including roles, permissions, and avatar</param>
/// <param name="AccessToken">JWT access token for authenticating API requests</param>
/// <param name="AccessTokenExpiresAt">Date and time when the access token expires in UTC</param>
/// <param name="RefreshToken">Refresh token for obtaining new access tokens</param>
/// <param name="RefreshTokenExpiresAt">Date and time when the refresh token expires in UTC</param>
/// <param name="TokenType">Type of token (typically "Bearer")</param>
public record AuthenticationDto(
    UserResponseDto User,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType = "Bearer"
);
