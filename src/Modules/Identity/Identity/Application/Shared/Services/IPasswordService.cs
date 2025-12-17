namespace _116.Identity.Application.Shared.Services;

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
}
