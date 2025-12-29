using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.Contracts;

/// <summary>
/// Factory for handling admin forgot password OTP creation and persistence.
/// </summary>
public interface IAdminForgotPasswordOtpFactory
{
    /// <summary>
    /// Creates and persists an OTP for admin password reset.
    /// </summary>
    /// <param name="userId">The admin user ID to create OTP for.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created OTP entity.</returns>
    Task<OtpEntity> CreatePasswordResetOtpAsync(
        Guid userId,
        CancellationToken cancellationToken
    );
}
