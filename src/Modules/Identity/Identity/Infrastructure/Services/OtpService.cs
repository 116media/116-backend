using System.Security.Cryptography;
using System.Text;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IOtpService" /> for OTP generation and management operations.
/// Codes are stored as HMAC-SHA256 hashes keyed with the <c>OTP_PEPPER</c> secret.
/// </summary>
public class OtpService : IOtpService
{
    /// <summary>
    /// Prefix identifying the current OTP hash scheme.
    /// </summary>
    private const string CurrentPrefix = "h1:";

    /// <summary>
    /// Exclusive upper bound of the generated code range, derived from the configured code length.
    /// </summary>
    private static readonly int CodeUpperBound = (int)Math.Pow(10, y: UserConstants.OtpCodeLength);

    private readonly byte[] _pepper;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes the service with an explicit hashing key, failing fast when none is supplied.
    /// </summary>
    /// <param name="pepper">The server-side key mixed into every code hash.</param>
    /// <param name="timeProvider">The clock the expiration window is measured from.</param>
    /// <exception cref="InvalidOperationException">Thrown when the pepper is missing or empty.</exception>
    public OtpService(string? pepper, TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(value: pepper))
        {
            throw new InvalidOperationException("OTP_PEPPER env variable is missing or empty.");
        }

        _pepper = Encoding.UTF8.GetBytes(s: pepper);
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public string GenerateOtpCode()
    {
        return RandomNumberGenerator
            .GetInt32(0, toExclusive: CodeUpperBound)
            .ToString($"D{UserConstants.OtpCodeLength}");
    }

    /// <inheritdoc />
    public OtpCreationResult CreateOtp(Guid userId, EnumOtpPurpose purpose)
    {
        string plainCode = GenerateOtpCode();
        string codeHash = Hash(code: plainCode);
        DateTime expiresAt = CalculateExpirationTime();

        OtpEntity otp = OtpEntity.Create(
            Guid.NewGuid(),
            userId: userId,
            codeHash: codeHash,
            purpose: purpose,
            expiresAt: expiresAt
        );

        return new OtpCreationResult(Otp: otp, PlainCode: plainCode);
    }

    /// <inheritdoc />
    public DateTime CalculateExpirationTime()
    {
        return _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(value: UserConstants.OtpExpirationMinutes);
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
