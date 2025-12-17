using _116.Identity.Application.Shared.Validators;

using FluentValidation;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.VerifyOtp;

/// <summary>
/// Validator for the <see cref="AdminVerifyOtpCommand"/> ensuring proper OTP verification data format.
/// </summary>
/// <remarks>
/// Validates email, OTP code, and purpose according to format requirements:
/// - Email: Valid email format
/// - Code: 6-digit numeric code
/// - Purpose: Must be EmailVerification or AccountRecovery
/// </remarks>
public class AdminVerifyOtpValidator : AbstractValidator<AdminVerifyOtpCommand>
{
    /// <summary>
    /// Configure validation rules for admin OTP verification.
    /// </summary>
    public AdminVerifyOtpValidator()
    {
        // Email validation
        RuleFor(x => x.Email).EmailValidation();
        // OTP code validation
        RuleFor(x => x.Code).OtpCodeValidation();
        // Purpose validation
        RuleFor(x => x.Purpose).OtpPurposeValidation();
    }
}
