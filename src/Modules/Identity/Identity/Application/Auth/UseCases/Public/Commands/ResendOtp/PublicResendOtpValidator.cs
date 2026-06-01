using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public PublicResendOtpValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Email).ValidEmail(i18n.User.Validation);
        RuleFor(x => x.Purpose).ValidOtpPurpose(i18n.User.Validation);
    }
}
