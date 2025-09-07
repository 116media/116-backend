using _116.Shared.Contracts.Application.CQRS;
using _116.User.Domain.Results;

namespace _116.User.Application.Public.UseCases.Commands.ForgotPassword;

/// <summary>
/// Command for initiating password reset process for existing users.
/// </summary>
/// <param name="Email">The user's registered email address.</param>
/// <remarks>
/// This command generates an OTP for password reset if a valid and active user account exists.
/// Returns authentication result and requires verification through OTP.
/// </remarks>
public record ForgotPasswordCommand(
    string Email
) : ICommand<ForgotPasswordResult>;

/// <summary>
/// Result of the <see cref="ForgotPasswordCommand"/> containing password reset details.
/// </summary>
/// <param name="AuthenticationResult">The authentication result with user info and JWT token.</param>
/// <param name="VerificationRequired">Always true - OTP verification is required for password reset.</param>
/// <remarks>
/// Contains authentication information for the password reset flow.
/// The user must verify the OTP sent to their email before proceeding with password reset.
/// </remarks>
public record ForgotPasswordResult(
    AuthenticationResult AuthenticationResult,
    bool VerificationRequired
);
