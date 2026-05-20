using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;

/// <summary>
/// Validator for the <see cref="AdminResetPasswordCommand" /> ensuring proper password reset data format.
/// </summary>
/// <remarks>
/// Validates email, OTP code, and new password according to security requirements:
/// - Email: Valid email format and required
/// - Code: 6-digit numeric OTP code
/// - NewPassword: Strong password with mixed cases, numbers, minimum length
/// </remarks>
public class AdminResetPasswordValidator : AbstractValidator<AdminResetPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminResetPasswordValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminResetPasswordValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Email).ValidEmail(msg);
        RuleFor(x => x.Code).ValidOtpCode(msg);
        RuleFor(x => x.NewPassword).ValidPassword(msg, "New password");
    }
}
