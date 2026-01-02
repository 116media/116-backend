using _116.Identity.Application.Shared.DTOs;

namespace _116.Identity.Domain.Results;

/// <summary>
/// Data transfer object representing the result of a successful authentication operation.
/// Contains user information, access token (JWT), and refresh token for session management.
/// </summary>
/// <param name="User">Complete user information including roles, permissions, and avatar</param>
/// <param name="AccessToken">JWT access token for authenticating API requests</param>
/// <param name="AccessTokenExpiresAt">Date and time when the access token expires in UTC</param>
/// <param name="RefreshToken">Refresh token for obtaining new access tokens</param>
/// <param name="RefreshTokenExpiresAt">Date and time when the refresh token expires in UTC</param>
/// <param name="TokenType">Type of token (typically "Bearer")</param>
public record AuthenticationResult(
    UserResponseDto User,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType = "Bearer"
);
