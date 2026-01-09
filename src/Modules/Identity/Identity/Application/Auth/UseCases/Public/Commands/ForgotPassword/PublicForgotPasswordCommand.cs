using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;

/// <summary>
/// Command for initiating the password reset process for existing users.
/// </summary>
/// <param name="Email">The user's registered email address.</param>
/// <remarks>
/// This command generates an OTP for password reset if a valid and active user account exists.
/// Returns authentication result and requires verification through OTP.
/// </remarks>
public record PublicForgotPasswordCommand(string Email) : ICommand<PublicForgotPasswordResult>;

/// <summary>
/// Result of the <see cref="PublicForgotPasswordCommand" /> containing password reset status.
/// </summary>
/// <param name="IsSuccess">Always true for security reasons to prevent user enumeration.</param>
/// <param name="Email">The email address from the request for client reference.</param>
/// <remarks>
/// Returns success regardless of whether the email exists to prevent user enumeration attacks.
/// The actual OTP is only generated and sent if a valid, active account exists.
/// The email is returned to help the client track which email address the reset was initiated for.
/// </remarks>
public record PublicForgotPasswordResult(bool IsSuccess, string Email);
