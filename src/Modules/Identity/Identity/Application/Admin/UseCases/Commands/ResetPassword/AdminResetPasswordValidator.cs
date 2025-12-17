using FluentValidation;
using _116.Identity.Application.Shared.Validators;

namespace _116.Identity.Application.Admin.UseCases.Commands.ResetPassword;

/// <summary>
/// Validator for the <see cref="AdminResetPasswordCommand"/> ensuring proper password reset data format.
/// </summary>
/// <remarks>
/// Validates email, OTP code, and new password according to security requirements:
/// - Email: Valid email format and required
/// - Code: 6-digit numeric OTP code
/// - NewPassword: Strong password with mixed cases, numbers, minimum length
/// </remarks>
public partial class AdminResetPasswordValidator : AbstractValidator<AdminResetPasswordCommand>
{
    /// <summary>
    /// Configure validation rules for admin password reset.
    /// </summary>
    public AdminResetPasswordValidator()
    {
        // Email validation
        RuleFor(x => x.Email).EmailValidation();

        // OTP code validation
        RuleFor(x => x.Code).OtpCodeValidation();

        // New password validation - strong password requirements
        RuleFor(x => x.NewPassword).PasswordValidation(fieldName: "New password");
    }
}
