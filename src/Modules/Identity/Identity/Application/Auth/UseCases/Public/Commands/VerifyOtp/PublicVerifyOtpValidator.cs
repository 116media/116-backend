using _116.Identity.Application.Auth.Validators;

using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;

/// <summary>
/// Validator for the <see cref="PublicVerifyOtpCommand" /> ensuring proper OTP verification data format.
/// </summary>
/// <remarks>
/// Validates email, OTP code, and purpose according to format requirements:
/// - Email: Valid email format
/// - Code: 6-digit numeric code
/// - Purpose: Must be EmailVerification or AccountRecovery
/// </remarks>
public class PublicVerifyOtpValidator : AbstractValidator<PublicVerifyOtpCommand>
{
    /// <summary>
    /// Configure validation rules for OTP verification.
    /// </summary>
    public PublicVerifyOtpValidator()
    {
        // Email validation
        RuleFor(x => x.Email).EmailValidation();
        // OTP code validation
        RuleFor(x => x.Code).OtpCodeValidation();
        // Purpose validation
        RuleFor(x => x.Purpose).OtpPurposeValidation();
    }
}
