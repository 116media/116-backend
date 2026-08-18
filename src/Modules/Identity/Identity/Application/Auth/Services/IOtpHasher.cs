namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Hashes OTP codes for storage. Kept separate from <see cref="IPasswordService" /> because the two
/// defend against different threats: a short numeric code cannot be protected by cost, so it is
/// keyed with a server-side pepper and metered by lockout instead.
/// </summary>
public interface IOtpHasher
{
    /// <summary>
    /// Hashes a plaintext OTP code for storage.
    /// </summary>
    /// <param name="code">The plaintext code.</param>
    /// <returns>The keyed hash to persist.</returns>
    string Hash(string code);

    /// <summary>
    /// Verifies a supplied code against a stored hash in constant time.
    /// </summary>
    /// <param name="code">The supplied code.</param>
    /// <param name="hash">The stored hash.</param>
    /// <returns>True when the code matches.</returns>
    bool Verify(string code, string? hash);
}
