using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IOtpService" /> for OTP generation and management operations.
/// </summary>
public class OtpService : IOtpService
{
    private readonly Random _random = new();

    /// <inheritdoc />
    public string GenerateOtpCode()
    {
        // Generate a random numeric OTP code
        string code = _random
            .Next(0, (int)Math.Pow(10, y: UserConstants.OtpCodeLength))
            .ToString($"D{UserConstants.OtpCodeLength}");
        return code;
    }

    /// <inheritdoc />
    public OtpEntity CreateOtp(Guid userId, EnumOtpPurpose purpose)
    {
        string code = GenerateOtpCode();
        DateTime expiresAt = CalculateExpirationTime();
        return OtpEntity.Create(
            Guid.NewGuid(),
            userId: userId,
            code: code,
            purpose: purpose,
            expiresAt: expiresAt
        );
    }

    /// <inheritdoc />
    public DateTime CalculateExpirationTime()
    {
        return DateTime.UtcNow.AddMinutes(value: UserConstants.OtpExpirationMinutes);
    }
}
