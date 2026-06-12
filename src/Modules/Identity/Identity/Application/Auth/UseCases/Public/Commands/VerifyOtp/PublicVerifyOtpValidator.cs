using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="PublicVerifyOtpValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicVerifyOtpValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.Email).ValidEmail(i18n);
        RuleFor(x => x.Code).ValidOtpCode(i18n);
        RuleFor(x => x.Purpose).ValidOtpPurpose(i18n);
    }
}
