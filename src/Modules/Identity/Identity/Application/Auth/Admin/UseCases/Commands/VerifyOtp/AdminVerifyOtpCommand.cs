using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.VerifyOtp;

/// <summary>
/// Command for verifying an OTP code for admin account verification.
/// </summary>
/// <param name="Email">The admin user's email address.</param>
/// <param name="Code">The OTP code to verify.</param>
/// <param name="Purpose">The purpose for which the OTP is being verified (EmailVerification or AccountRecovery).</param>
/// <remarks>
/// This command is used to verify the OTP code sent to the admin user's email for various purposes.
/// Upon successful verification, the admin user's account will be marked as verified for email verification purpose.
/// </remarks>
public record AdminVerifyOtpCommand(
    string Email,
    string Code,
    string Purpose
) : ICommand<AdminVerifyOtpResult>;
/// <summary>
/// Result of the <see cref="AdminVerifyOtpCommand"/> containing verification status.
/// </summary>
/// <param name="IsSuccess">Indicates whether the OTP verification was successful.</param>
/// <remarks>
/// Contains the verification result and a user-friendly message explaining the outcome.
/// </remarks>
public record AdminVerifyOtpResult(
    bool IsSuccess
);
