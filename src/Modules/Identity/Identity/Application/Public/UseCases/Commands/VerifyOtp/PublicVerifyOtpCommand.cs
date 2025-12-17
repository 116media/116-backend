using _116.Shared.Contracts.Application.CQRS;

namespace _116.Auth.Application.Public.UseCases.Commands.VerifyOtp;

/// <summary>
/// Command for verifying an OTP code for user account verification.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Code">The OTP code to verify.</param>
/// <param name="Purpose">The purpose for which the OTP is being verified (EmailVerification or AccountRecovery).</param>
/// <remarks>
/// This command is used to verify the OTP code sent to the user's email for various purposes.
/// Upon successful verification, the user's account will be marked as verified for email verification purpose.
/// </remarks>
public record PublicVerifyOtpCommand(
    string Email,
    string Code,
    string Purpose
) : ICommand<PublicVerifyOtpResult>;

/// <summary>
/// Result of the <see cref="PublicVerifyOtpCommand"/> containing verification status.
/// </summary>
/// <param name="IsSuccess">Indicates whether the OTP verification was successful.</param>
/// <remarks>
/// Contains the verification result and a user-friendly message explaining the outcome.
/// </remarks>
public record PublicVerifyOtpResult(
    bool IsSuccess
);
