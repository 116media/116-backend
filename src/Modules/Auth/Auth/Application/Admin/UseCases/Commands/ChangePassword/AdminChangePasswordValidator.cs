using FluentValidation;
using _116.Auth.Application.Shared.Validators;

namespace _116.Auth.Application.Admin.UseCases.Commands.ChangePassword;

/// <summary>
/// Validator for the <see cref="AdminChangePasswordCommand"/> ensuring proper password change data format.
/// </summary>
/// <remarks>
/// Validates old password and new password according to security requirements:
/// - OldPassword: Required for current password verification
/// - NewPassword: Strong password with mixed cases, numbers, minimum length
/// - NewPassword: Must be different from old password (business logic validation)
/// </remarks>
public partial class AdminChangePasswordValidator : AbstractValidator<AdminChangePasswordCommand>
{
    /// <summary>
    /// Configure validation rules for admin password change.
    /// </summary>
    public AdminChangePasswordValidator()
    {
        // Old password validation - required for verification
        RuleFor(x => x.OldPassword).OldPasswordValidation();

        // New password validation - strong password requirements
        RuleFor(x => x.NewPassword).PasswordValidation(fieldName: "New password");
    }
}
