using System.Security.Cryptography;
using _116.Identity.Application.Auth.Services;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Implementation of password service using PBKDF2 hashing algorithm with SHA-256.
/// Hashes are version-prefixed so the work factor can be raised without invalidating stored
/// credentials: <c>v2:</c> is written today and <c>v1:</c> is still read for accounts that have not
/// logged in since the change.
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
    /// PBKDF2 iterations behind the original <c>v1:</c> prefix.
    /// </summary>
    private const int LegacyIterations = 25_000;

    /// <summary>
    /// PBKDF2 iterations for newly written hashes, per current OWASP guidance for SHA-256.
    /// </summary>
    private const int CurrentIterations = 600_000;

    /// <summary>
    /// Prefix identifying a hash written at the legacy work factor.
    /// </summary>
    private const string LegacyPrefix = "v1:";

    /// <summary>
    /// Prefix identifying a hash written at the current work factor.
    /// </summary>
    private const string CurrentPrefix = "v2:";

    /// <summary>
    /// A hash of a value nobody can supply, used to spend the same work on an account that has no
    /// password as on one that does. Built once per process so the padding costs one derivation.
    /// </summary>
    private static readonly Lazy<string> DummyHash = new(() =>
        new PasswordService().Hash(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)))
    );

    /// <inheritdoc />
    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(count: SaltSize);
        byte[] hash = Derive(password: password, salt: salt, iterations: CurrentIterations);

        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(sourceArray: salt, 0, destinationArray: hashBytes, 0, length: SaltSize);
        Array.Copy(sourceArray: hash, 0, destinationArray: hashBytes, destinationIndex: SaltSize, length: HashSize);

        return $"{CurrentPrefix}{Convert.ToBase64String(inArray: hashBytes)}";
    }

    /// <inheritdoc />
    public bool Verify(string password, string? hash)
    {
        int iterations = ResolveIterations(hash: hash);
        if (iterations == 0)
        {
            return false;
        }

        try
        {
            byte[] hashBytes = Convert.FromBase64String(hash![3..]);
            if (hashBytes.Length != SaltSize + HashSize)
            {
                return false;
            }

            byte[] salt = new byte[SaltSize];
            byte[] storedHash = new byte[HashSize];
            Array.Copy(sourceArray: hashBytes, 0, destinationArray: salt, 0, length: SaltSize);
            Array.Copy(
                sourceArray: hashBytes,
                sourceIndex: SaltSize,
                destinationArray: storedHash,
                0,
                length: HashSize
            );

            byte[] computedHash = Derive(password: password, salt: salt, iterations: iterations);
            return CryptographicOperations.FixedTimeEquals(left: storedHash, right: computedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public bool VerifyOrDummy(string password, string? hash)
    {
        if (ResolveIterations(hash: hash) != 0)
        {
            return Verify(password: password, hash: hash);
        }

        // The result is discarded: the call exists so a missing account costs a real derivation.
        Verify(password: password, hash: DummyHash.Value);
        return false;
    }

    /// <inheritdoc />
    public bool NeedsRehash(string? hash)
    {
        return !string.IsNullOrWhiteSpace(value: hash)
            && !hash.StartsWith(value: CurrentPrefix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Maps a stored hash to the iteration count its prefix declares.
    /// </summary>
    /// <param name="hash">The stored hash.</param>
    /// <returns>The iteration count, or zero when the hash is missing or unrecognised.</returns>
    private static int ResolveIterations(string? hash)
    {
        if (string.IsNullOrWhiteSpace(value: hash))
        {
            return 0;
        }

        if (hash.StartsWith(value: CurrentPrefix, StringComparison.Ordinal))
        {
            return CurrentIterations;
        }

        return hash.StartsWith(value: LegacyPrefix, StringComparison.Ordinal) ? LegacyIterations : 0;
    }

    /// <summary>
    /// Derives the PBKDF2 hash for a password, salt and work factor.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="salt">The salt bytes.</param>
    /// <param name="iterations">The iteration count to apply.</param>
    /// <returns>The derived hash bytes.</returns>
    private static byte[] Derive(string password, byte[] salt, int iterations)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: HashSize
        );
    }
}
