using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.Contracts;

/// <summary>
/// Factory for handling admin OTP resend logic.
/// </summary>
public interface IAdminResendOtpFactory
{
    /// <summary>
    /// Invalidates existing OTPs and creates a new OTP for the specified purpose.
    /// </summary>
    /// <param name="userId">The admin user ID to create OTP for.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// The newly persisted OTP entity together with the plaintext code the caller has to deliver.
    /// </returns>
    Task<OtpCreationResult> ResendOtpAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken);
}
