using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="AdminChangePasswordValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">Validation error messages for rule configuration.</param>
    public AdminChangePasswordValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.OldPassword).ValidOldPassword(msg);
        RuleFor(x => x.NewPassword).ValidPassword(msg, "New password");
    }
}
