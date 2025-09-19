using FluentValidation;
using _116.User.Application.Shared.Validators;

namespace _116.User.Application.Admin.UseCases.Commands.ChangePassword;

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
        RuleFor(x => x.OldPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Current password is required");

        // New password validation - strong password requirements
        RuleFor(x => x.NewPassword).PasswordValidation(fieldName: "New password");

        // Cross-field validation - new password must be different from old password
        RuleFor(x => x)
            .Must(command => command.OldPassword != command.NewPassword)
            .WithMessage("New password must be different from current password")
            .WithName("NewPassword");
    }

}
