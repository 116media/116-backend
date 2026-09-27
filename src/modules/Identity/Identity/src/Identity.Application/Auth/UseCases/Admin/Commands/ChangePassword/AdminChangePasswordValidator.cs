using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminChangePasswordValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.OldPassword).ValidOldPassword(i18n.User.Validation);
        RuleFor(x => x.NewPassword)
            .ValidPassword(i18n.User.Validation, fieldName: i18n.User.Validation.NewPasswordFieldName());
    }
}
