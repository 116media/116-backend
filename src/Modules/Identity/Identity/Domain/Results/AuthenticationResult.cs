using _116.Auth.Domain.DTOs;

namespace _116.Auth.Domain.Results;

/// <summary>
/// Data transfer object representing the result of a successful authentication operation.
/// Contains both user information and the JWT token for API access.
/// </summary>
/// <param name="User">Complete user information including roles, permissions, and avatar</param>
/// <param name="Token">JWT token for authenticating further API requests</param>
/// <param name="ExpiresAt">Date and time when the token expires in UTC</param>
/// <param name="TokenType">Type of token (typically "Bearer")</param>
public record AuthenticationResult(
    UserResponseDto User,
    string Token,
    DateTime ExpiresAt,
    string TokenType = "Bearer"
);
