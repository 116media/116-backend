using System.Security.Cryptography;

using _116.Identity.Application.Shared.Services;

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
    private const int TokenSize = 32;
    /// <summary>
    /// Size of the salt in bytes used for token hashing.
    /// </summary>
    private const int SaltSize = 16;
    /// <summary>
    /// Size of the resulting hash in bytes.
    /// </summary>
    private const int HashSize = 32;
    /// <summary>
    /// Number of iterations for PBKDF2 algorithm to ensure computational cost.
    /// </summary>
    private const int Iterations = 100000;

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        byte[] tokenBytes = new byte[TokenSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }

    /// <inheritdoc />
    public string HashRefreshToken(string refreshToken)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            refreshToken,
            salt,
            Iterations,
            HashAlgorithmName.SHA256
        );
        byte[] hash = pbkdf2.GetBytes(HashSize);

        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    /// <inheritdoc />
    public bool VerifyRefreshToken(string refreshToken, string hash)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        byte[] hashBytes = Convert.FromBase64String(hash);
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            refreshToken,
            salt,
            Iterations,
            HashAlgorithmName.SHA256
        );
        byte[] computedHash = pbkdf2.GetBytes(HashSize);

        for (int i = 0; i < HashSize; i++)
        {
            if (hashBytes[i + SaltSize] != computedHash[i])
            {
                return false;
            }
        }

        return true;
    }
}
