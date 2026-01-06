namespace _116.Identity.Domain.Results;

/// <summary>
/// Represents the result of JWT token generation including the token and its expiration time.
/// </summary>
/// <param name="Token">The generated JWT token string</param>
/// <param name="ExpiresAt">The UTC date and time when the token expires</param>
public record JwtGenerationResult(string Token, DateTime ExpiresAt);
