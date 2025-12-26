using _116.Identity.Application.Auth.Validators;

using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword;

/// <summary>
/// Validator for the <see cref="AdminChangePasswordCommand" /> ensuring proper password change data format.
/// </summary>
/// <remarks>
/// Validates old password and new password according to security requirements:
/// - OldPassword: Required for current password verification
/// - NewPassword: Strong password with mixed cases, numbers, minimum length
/// - NewPassword: Must be different from old password (business logic validation)
/// </remarks>
public class AdminChangePasswordValidator : AbstractValidator<AdminChangePasswordCommand>
{
    /// <summary>
    /// Configure validation rules for admin password change.
    /// </summary>
    public AdminChangePasswordValidator()
    {
        // Old password validation - required for verification
        RuleFor(x => x.OldPassword).OldPasswordValidation();
        // New password validation - strong password requirements
        RuleFor(x => x.NewPassword).PasswordValidation("New password");
    }
}
