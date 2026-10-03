using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// The OTP consumption protocol shared by the VerifyOtp handlers: load the outstanding code,
/// judge the presented one, consume it, and retire its siblings and the failure counter.
/// </summary>
public interface IOtpVerificationService
{
    /// <summary>
    /// Consumes the outstanding OTP for the account and purpose. Returns only when the presented
    /// code is valid; otherwise meters the missed attempt, commits it, and throws the matching
    /// localized error. The caller owns the success-path commit.
    /// </summary>
    /// <param name="userId">The account presenting the code.</param>
    /// <param name="purpose">The purpose the code was issued for.</param>
    /// <param name="code">The presented code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task ConsumeOtpAsync(Guid userId, OtpPurpose purpose, string code, CancellationToken cancellationToken);
}
