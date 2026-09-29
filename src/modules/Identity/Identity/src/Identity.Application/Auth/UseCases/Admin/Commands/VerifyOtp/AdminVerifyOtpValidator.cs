using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;

/// <summary>
/// Validator for the <see cref="AdminVerifyOtpCommand" /> ensuring proper OTP verification data format.
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
    /// Initializes a new instance of <see cref="AdminVerifyOtpValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminVerifyOtpValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Email).ValidEmail(i18n.User.Validation);
        RuleFor(x => x.Code).ValidOtpCode(i18n.User.Validation);
        RuleFor(x => x.Purpose).ValidOtpPurpose(i18n.User.Validation);
    }
}
