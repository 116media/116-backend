using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="PublicResendOtpValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicResendOtpValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Email).ValidEmail(msg);
        RuleFor(x => x.Purpose).ValidOtpPurpose(msg);
    }
}
