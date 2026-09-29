using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.Factories.Contracts;

/// <summary>
/// Shared verification step for the VerifyOtp handlers: compares the presented code and throws
/// the localized error for every non-valid verification status.
/// </summary>
public interface IOtpVerificationFactory
{
    /// <summary>
    /// Validates the presented code against the outstanding OTP; returns only when the code is
    /// valid, otherwise meters the missed attempt and throws the matching localized error.
    /// </summary>
    /// <param name="otp">The outstanding OTP.</param>
    /// <param name="code">The presented code.</param>
    /// <param name="userId">The account failures are metered against.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task ValidateOtpAsync(OtpEntity otp, string code, Guid userId, CancellationToken cancellationToken);
}
