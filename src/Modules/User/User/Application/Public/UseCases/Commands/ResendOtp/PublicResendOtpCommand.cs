using _116.Shared.Contracts.Application.CQRS;
using _116.User.Domain.Enums;

namespace _116.User.Application.Public.UseCases.Commands.ResendOtp;

/// <summary>
/// Command for resending OTP codes to public users.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Purpose">The purpose for which the OTP is being resent.</param>
/// <remarks>
/// This command invalidates any existing OTPs for the specified purpose and generates a new one.
/// Used for email verification, password reset, and other verification scenarios where the user didn't receive the initial OTP.
/// </remarks>
public record PublicResendOtpCommand(
    string Email,
    OtpPurpose Purpose
) : ICommand<PublicResendOtpResult>;

/// <summary>
/// Result of the <see cref="PublicResendOtpCommand"/> containing OTP resend status.
/// </summary>
/// <param name="IsSuccess">Always true for security reasons to prevent user enumeration.</param>
/// <param name="Message">A generic message indicating the operation status.</param>
/// <remarks>
/// Returns success regardless of whether the email exists to prevent user enumeration attacks.
/// The actual OTP is only generated and sent if a valid, active account exists.
/// </remarks>
public record PublicResendOtpResult(
    bool IsSuccess
);
