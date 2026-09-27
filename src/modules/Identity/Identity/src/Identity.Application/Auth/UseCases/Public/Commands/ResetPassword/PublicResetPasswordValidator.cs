using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;

/// <summary>
/// Validator for the <see cref="PublicResetPasswordCommand" /> ensuring proper password reset data format.
/// </summary>
/// <remarks>
/// Validates email, OTP code, and new password according to security requirements:
/// - Email: Valid email format and required
/// - Code: 6-digit numeric OTP code
/// - NewPassword: Strong password with mixed cases, numbers, minimum length
/// </remarks>
public class PublicResetPasswordValidator : AbstractValidator<PublicResetPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicResetPasswordValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public PublicResetPasswordValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Email).ValidEmail(i18n.User.Validation);
        RuleFor(x => x.Code).ValidOtpCode(i18n.User.Validation);
        RuleFor(x => x.NewPassword)
            .ValidPassword(i18n.User.Validation, fieldName: i18n.User.Validation.NewPasswordFieldName());
    }
}
