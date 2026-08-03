using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// The outcome of creating an OTP: the entity that is safe to persist, paired with the
/// plaintext code that has to reach the user.
/// </summary>
/// <param name="Otp">
/// The OTP entity carrying only the hashed code; this is the value that gets persisted.
/// </param>
/// <param name="PlainCode">
/// The deliverable code. It exists in memory for the duration of the creating flow and may only
/// be handed to the mailer tokens; it is never persisted, logged, or placed on an event payload.
/// </param>
public record OtpCreationResult(OtpEntity Otp, string PlainCode);

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
    /// Creates a new OTP entity with a generated code and expiration.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <returns>
    /// The entity ready to be saved together with the plaintext code to deliver.
    /// </returns>
    /// <remarks>
    /// The entity carries only the hash of the generated code, so the caller has to take the
    /// plaintext from the result to build the outgoing email.
    /// </remarks>
    OtpCreationResult CreateOtp(Guid userId, EnumOtpPurpose purpose);

    /// <summary>
    /// Calculates the expiration time for an OTP.
    /// </summary>
    /// <returns>The expiration DateTime in UTC.</returns>
    /// <remarks>
    /// Uses the expiration minutes defined in UserConstants.OtpExpirationMinutes.
    /// </remarks>
    DateTime CalculateExpirationTime();
}
