using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;

/// <summary>
/// Command for resetting a user's password using OTP verification.
/// </summary>
/// <param name="Email">The user's registered email address.</param>
/// <param name="Code">The OTP code received for password reset.</param>
/// <param name="NewPassword">The new password to set for the user.</param>
/// <remarks>
/// This command resets a user's password after validating the OTP sent during the forgot password process.
/// The OTP must be valid, not expired, and match the user's email address.
/// </remarks>
public record PublicResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword
) : ICommand<PublicResetPasswordResult>;

/// <summary>
/// Result of the <see cref="PublicResetPasswordCommand" /> containing reset status.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password reset was successful.</param>
/// <remarks>
/// Simple result indicating successful password reset operation.
/// Upon success, the user can log in with their new password.
/// </remarks>
public record PublicResetPasswordResult(bool IsSuccess);
