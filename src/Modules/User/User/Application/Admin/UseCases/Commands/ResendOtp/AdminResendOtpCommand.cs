using _116.Shared.Contracts.Application.CQRS;
using _116.User.Domain.Enums;

namespace _116.User.Application.Admin.UseCases.Commands.ResendOtp;

/// <summary>
/// Command for resending OTP codes to admin users.
/// </summary>
/// <param name="Email">The admin user's email address.</param>
/// <param name="Purpose">The purpose for which the OTP is being resent.</param>
/// <remarks>
/// This command invalidates any existing OTPs for the specified purpose and generates a new one.
/// Used for email verification, password reset, and other verification scenarios where the admin didn't receive the initial OTP.
/// </remarks>
public record AdminResendOtpCommand(
    string Email,
    OtpPurpose Purpose
) : ICommand<AdminResendOtpResult>;

/// <summary>
/// Result of the <see cref="AdminResendOtpCommand"/> containing OTP resend status.
/// </summary>
/// <param name="IsSuccess">Indicates whether the OTP was successfully resent.</param>
/// <remarks>
/// Returns success when a new OTP has been generated and the previous ones invalidated.
/// The actual OTP is only generated and sent if a valid, active admin account exists.
/// </remarks>
public record AdminResendOtpResult(
    bool IsSuccess
);
