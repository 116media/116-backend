using System.Security.Cryptography;
using System.Text;
using _116.Identity.Application.Auth.Services;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Implementation of refresh token service using cryptographically secure random generation
/// and PBKDF2 hashing algorithm with SHA-256 for secure storage.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    /// <summary>
    /// Size of the refresh token in bytes.
    /// </summary>
    private const int TokenSize = 256;

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        byte[] tokenBytes = new byte[TokenSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data: tokenBytes);
        return Convert.ToBase64String(inArray: tokenBytes);
    }

    /// <inheritdoc />
    public string HashRefreshToken(string refreshToken)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
