using _116.Shared.Contracts.Application.CQRS;

namespace _116.Auth.Application.Public.UseCases.Commands.ForgotPassword;

/// <summary>
/// Command for initiating the password reset process for existing users.
/// </summary>
/// <param name="Email">The user's registered email address.</param>
/// <remarks>
/// This command generates an OTP for password reset if a valid and active user account exists.
/// Returns authentication result and requires verification through OTP.
/// </remarks>
public record PublicForgotPasswordCommand(
    string Email
) : ICommand<PublicForgotPasswordResult>;

/// <summary>
/// Result of the <see cref="PublicForgotPasswordCommand"/> containing password reset status.
/// </summary>
/// <param name="IsSuccess">Always true for security reasons to prevent user enumeration.</param>
/// <remarks>
/// Returns success regardless of whether the email exists to prevent user enumeration attacks.
/// The actual OTP is only generated and sent if a valid, active account exists.
/// </remarks>
public record PublicForgotPasswordResult(
    bool IsSuccess
);
