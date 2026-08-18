using System.Security.Cryptography;
using System.Text;
using _116.Identity.Application.Auth.Services;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// HMAC-SHA256 implementation of <see cref="IOtpHasher" />, keyed with the <c>OTP_PEPPER</c> secret.
/// </summary>
public class OtpHasher : IOtpHasher
{
    /// <summary>
    /// Prefix identifying the current OTP hash scheme.
    /// </summary>
    private const string CurrentPrefix = "h1:";

    private readonly byte[] _pepper;

    /// <summary>
    /// Initializes the hasher with an explicit key, failing fast when none is supplied.
    /// </summary>
    /// <param name="pepper">The server-side key mixed into every hash.</param>
    /// <exception cref="InvalidOperationException">Thrown when the pepper is missing or empty.</exception>
    public OtpHasher(string? pepper)
    {
        if (string.IsNullOrWhiteSpace(value: pepper))
        {
            throw new InvalidOperationException("OTP_PEPPER env variable is missing or empty.");
        }

        _pepper = Encoding.UTF8.GetBytes(s: pepper);
    }

    /// <inheritdoc />
    public string Hash(string code)
    {
        return $"{CurrentPrefix}{Convert.ToBase64String(inArray: Compute(code: code))}";
    }

    /// <inheritdoc />
    public bool Verify(string code, string? hash)
    {
        if (string.IsNullOrWhiteSpace(value: hash) || !hash.StartsWith(value: CurrentPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            byte[] storedHash = Convert.FromBase64String(s: hash[CurrentPrefix.Length..]);
            return CryptographicOperations.FixedTimeEquals(left: storedHash, right: Compute(code: code));
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Computes the keyed hash of a code.
    /// </summary>
    /// <param name="code">The plaintext code.</param>
    /// <returns>The raw HMAC bytes.</returns>
    private byte[] Compute(string code)
    {
        return HMACSHA256.HashData(key: _pepper, source: Encoding.UTF8.GetBytes(s: code));
    }
}
