using System.Security.Cryptography;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IOtpService" /> for OTP generation and management operations.
/// </summary>
/// <param name="passwordService">Hashing service used to derive the stored OTP code hash.</param>
/// <param name="timeProvider">The clock the expiration window is measured from.</param>
public class OtpService(IPasswordService passwordService, TimeProvider timeProvider) : IOtpService
{
    /// <summary>
    /// Exclusive upper bound of the generated code range, derived from the configured code length.
    /// </summary>
    private static readonly int CodeUpperBound = (int)Math.Pow(10, y: UserConstants.OtpCodeLength);

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
        string codeHash = passwordService.Hash(password: plainCode);
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
        return timeProvider.GetUtcNow().UtcDateTime.AddMinutes(value: UserConstants.OtpExpirationMinutes);
    }
}
