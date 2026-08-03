using _116.Identity.Application.Auth.Services;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.Contracts;

/// <summary>
/// Factory for handling forgot password OTP creation and persistence.
/// </summary>
public interface IPublicForgotPasswordOtpFactory
{
    /// <summary>
    /// Creates and persists an OTP for password reset.
    /// </summary>
    /// <param name="userId">The user ID to create OTP for.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// The persisted OTP entity together with the plaintext code the caller has to deliver.
    /// </returns>
    Task<OtpCreationResult> CreatePasswordResetOtpAsync(Guid userId, CancellationToken cancellationToken);
}
