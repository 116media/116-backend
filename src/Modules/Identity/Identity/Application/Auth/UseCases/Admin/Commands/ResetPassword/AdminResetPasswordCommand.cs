using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;

/// <summary>
/// Command for resetting an admin user's password using OTP verification.
/// </summary>
/// <param name="Email">The admin user's registered email address.</param>
/// <param name="Code">The OTP code received for password reset.</param>
/// <param name="NewPassword">The new password to set for the admin user.</param>
/// <remarks>
/// This command resets an admin user's password after validating the OTP sent during the forgot password process.
/// The OTP must be valid, not expired, and match the admin user's email address.
/// </remarks>
public record AdminResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword
) : ICommand<AdminResetPasswordResult>;

/// <summary>
/// Result of the <see cref="AdminResetPasswordCommand" /> containing reset status.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password reset was successful.</param>
/// <remarks>
/// Simple result indicating successful password reset operation.
/// Upon success, the admin user can log in with their new password.
/// </remarks>
public record AdminResetPasswordResult(
    bool IsSuccess
);
