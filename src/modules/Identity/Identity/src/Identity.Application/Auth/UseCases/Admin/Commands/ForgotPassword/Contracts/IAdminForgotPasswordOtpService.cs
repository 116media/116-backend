using _116.Identity.Application.Auth.Ports;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.Contracts;

/// <summary>
/// Service for handling admin forgot password OTP creation and persistence.
/// </summary>
public interface IAdminForgotPasswordOtpService
{
    /// <summary>
    /// Creates and persists an OTP for admin password reset.
    /// </summary>
    /// <param name="userId">The admin user ID to create OTP for.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// The persisted OTP entity together with the plaintext code the caller has to deliver.
    /// </returns>
    Task<OtpCreationResult> CreatePasswordResetOtpAsync(Guid userId, CancellationToken cancellationToken);
}
