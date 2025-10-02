using FluentValidation;
using _116.Auth.Application.Shared.Validators;

namespace _116.Auth.Application.Public.UseCases.Commands.ResendOtp;

/// <summary>
/// Validator for the <see cref="PublicResendOtpCommand"/> ensuring proper resend OTP data format.
/// </summary>
/// <remarks>
/// Validates email format and OTP purpose according to requirements:
/// - Email: Valid email format and required
/// - Purpose: Must be a valid OTP purpose enum value
/// </remarks>
public class PublicResendOtpValidator : AbstractValidator<PublicResendOtpCommand>
{
    /// <summary>
    /// Configure validation rules for public OTP resend.
    /// </summary>
    public PublicResendOtpValidator()
    {
        // Email validation
        RuleFor(x => x.Email).EmailValidation();

        // Purpose validation - must be a valid enum value
        RuleFor(x => x.Purpose).OtpPurposeValidation();
    }
}
