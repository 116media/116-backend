namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Service for password hashing and verification operations.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a plain text password using a secure hashing algorithm.
    /// </summary>
    /// <param name="password">The plain text password to hash</param>
    /// <returns>The hashed password string</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain text password against a hashed password.
    /// </summary>
    /// <param name="password">The plain text password to verify</param>
    /// <param name="hash">The hashed password to verify against</param>
    /// <returns>True if the password matches the hash; otherwise, false</returns>
    bool Verify(string password, string? hash);

    /// <summary>
    /// Verifies against the stored hash, or against a constant hash of equal cost when the account
    /// has none, so an unknown account and a wrong password take the same time.
    /// </summary>
    /// <param name="password">The plain text password to verify</param>
    /// <param name="hash">The hashed password to verify against, or null</param>
    /// <returns>True only when a real hash was supplied and it matched</returns>
    bool VerifyOrDummy(string password, string? hash);

    /// <summary>
    /// Whether the stored hash was produced by an older work factor and should be replaced.
    /// </summary>
    /// <param name="hash">The stored hash</param>
    /// <returns>True when the hash is missing or not the current version</returns>
    bool NeedsRehash(string? hash);
}
