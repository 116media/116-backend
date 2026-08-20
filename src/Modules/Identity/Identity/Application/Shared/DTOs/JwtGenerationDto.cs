namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// Carries a generated JWT and its expiry from the token service to its callers.
/// </summary>
/// <param name="Token">The generated JWT token string</param>
/// <param name="ExpiresAt">The UTC date and time when the token expires</param>
public record JwtGenerationDto(string Token, DateTime ExpiresAt);
