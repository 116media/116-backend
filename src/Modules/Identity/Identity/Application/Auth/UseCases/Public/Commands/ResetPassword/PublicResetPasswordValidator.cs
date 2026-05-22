using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicResetPasswordValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Email)
            .ValidEmail(
                emailRequired: msg.EmailRequired(),
                emailTooLong: msg.EmailTooLong(UserConstants.MaxEmailLength),
                invalidEmailFormat: msg.InvalidEmailFormatMsg()
            );
        RuleFor(x => x.Code)
            .ValidOtpCode(
                otpCodeRequired: msg.OtpCodeRequired(),
                otpCodeWrongLength: msg.OtpCodeWrongLength(UserConstants.OtpCodeLength),
                otpCodeNotNumeric: msg.OtpCodeNotNumeric()
            );
        RuleFor(x => x.NewPassword)
            .ValidPassword(
                passwordRequired: msg.PasswordRequired(),
                passwordTooShort: msg.PasswordTooShort("New password", UserConstants.MinPasswordLength),
                passwordComplexity: msg.PasswordComplexity("New password")
            );
    }
}
