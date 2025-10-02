using _116.Shared.Contracts.Application.CQRS;

namespace _116.Auth.Application.Admin.UseCases.Commands.ForgotPassword;

/// <summary>
/// Command for initiating the password reset process for existing admin users.
/// </summary>
/// <param name="Email">The admin user's registered email address.</param>
/// <remarks>
/// This command generates an OTP for password reset if a valid and active admin user account exists.
/// Returns authentication result and requires verification through OTP.
/// </remarks>
public record AdminForgotPasswordCommand(
    string Email
) : ICommand<AdminForgotPasswordResult>;

/// <summary>
/// Result of the <see cref="AdminForgotPasswordCommand"/> containing password reset status.
/// </summary>
/// <param name="IsSuccess">Always true for security reasons to prevent user enumeration.</param>
/// <remarks>
/// Returns success regardless of whether the email exists to prevent user enumeration attacks.
/// The actual OTP is only generated and sent if a valid, active admin account exists.
/// </remarks>
public record AdminForgotPasswordResult(
    bool IsSuccess
);
