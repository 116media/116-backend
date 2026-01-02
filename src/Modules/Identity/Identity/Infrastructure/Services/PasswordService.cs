using System.Security.Cryptography;

using _116.Identity.Application.Auth.Services;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Implementation of password service using PBKDF2 hashing algorithm with SHA-256.
/// Provides secure password hashing and verification with salt-based protection.
/// </summary>
public class PasswordService : IPasswordService
{
    /// <summary>
    /// Size of the salt in bytes used for password hashing.
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
    public string Hash(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(data: salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password: password, salt: salt, iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(cb: HashSize);
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(sourceArray: salt, 0, destinationArray: hashBytes, 0, length: SaltSize);
        Array.Copy(sourceArray: hash, 0, destinationArray: hashBytes, destinationIndex: SaltSize, length: HashSize);
        return $"v1:{Convert.ToBase64String(inArray: hashBytes)}";
    }

    /// <inheritdoc />
    public bool Verify(string password, string? hash)
    {
        if (string.IsNullOrWhiteSpace(value: hash) || !hash.StartsWith("v1:"))
        {
            return false;
        }

        try
        {
            byte[] hashBytes = Convert.FromBase64String(hash[3..]);
            if (hashBytes.Length != SaltSize + HashSize)
            {
                return false;
            }

            byte[] salt = new byte[SaltSize];
            byte[] storedHash = new byte[HashSize];
            Array.Copy(sourceArray: hashBytes, 0, destinationArray: salt, 0, length: SaltSize);
            Array.Copy(sourceArray: hashBytes, sourceIndex: SaltSize, destinationArray: storedHash, 0,
                length: HashSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password: password, salt: salt, iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(cb: HashSize);
            return CryptographicOperations.FixedTimeEquals(left: storedHash, right: computedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
