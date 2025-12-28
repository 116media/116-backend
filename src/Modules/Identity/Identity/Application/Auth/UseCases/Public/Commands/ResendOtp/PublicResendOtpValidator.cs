using _116.Identity.Application.Auth.Validators;

using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp;

/// <summary>
/// Validator for the <see cref="PublicResendOtpCommand" /> ensuring proper resend OTP data format.
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
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Purpose).ValidOtpPurpose();
    }
}
