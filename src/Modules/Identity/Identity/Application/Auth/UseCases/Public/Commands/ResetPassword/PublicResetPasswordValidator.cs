using _116.Identity.Application.Auth.Validators;

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
    /// Configure validation rules for password reset.
    /// </summary>
    public PublicResetPasswordValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Code).ValidOtpCode();
        RuleFor(x => x.NewPassword).ValidPassword("New password");
    }
}
