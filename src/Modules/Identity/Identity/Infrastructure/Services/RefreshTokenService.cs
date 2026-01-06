using System.Security.Cryptography;
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
    private const int Iterations = 25000;

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
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(data: salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            salt: salt,
            password: refreshToken,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256
        );
        byte[] hash = pbkdf2.GetBytes(cb: HashSize);

        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(sourceArray: salt, 0, destinationArray: hashBytes, 0, length: SaltSize);
        Array.Copy(sourceArray: hash, 0, destinationArray: hashBytes, destinationIndex: SaltSize, length: HashSize);

        return Convert.ToBase64String(inArray: hashBytes);
    }

    /// <inheritdoc />
    public bool VerifyRefreshToken(string refreshToken, string hash)
    {
        if (string.IsNullOrWhiteSpace(value: refreshToken) || string.IsNullOrWhiteSpace(value: hash))
        {
            return false;
        }

        byte[] hashBytes = Convert.FromBase64String(s: hash);
        byte[] salt = new byte[SaltSize];
        Array.Copy(sourceArray: hashBytes, 0, destinationArray: salt, 0, length: SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            salt: salt,
            password: refreshToken,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256
        );
        byte[] computedHash = pbkdf2.GetBytes(cb: HashSize);

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
