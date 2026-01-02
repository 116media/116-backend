using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Service for OTP (One-Time Password) generation and management operations.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a new OTP code.
    /// </summary>
    /// <returns>A randomly generated OTP code.</returns>
    /// <remarks>
    /// The generated code will be numeric and have the length specified in UserConstants.OtpCodeLength.
    /// </remarks>
    string GenerateOtpCode();

    /// <summary>
    /// Creates a new OTP entity with generated code and expiration.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <returns>A new OTP entity ready to be saved.</returns>
    /// <remarks>
    /// This method generates a new OTP code and sets the expiration time based on UserConstants.OtpExpirationMinutes.
    /// </remarks>
    OtpEntity CreateOtp(Guid userId, EnumOtpPurpose purpose);

    /// <summary>
    /// Calculates the expiration time for an OTP.
    /// </summary>
    /// <returns>The expiration DateTime in UTC.</returns>
    /// <remarks>
    /// Uses the expiration minutes defined in UserConstants.OtpExpirationMinutes.
    /// </remarks>
    DateTime CalculateExpirationTime();
}
