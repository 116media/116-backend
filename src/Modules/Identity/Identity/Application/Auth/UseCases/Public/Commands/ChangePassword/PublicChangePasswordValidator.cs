using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword;

/// <summary>
/// Validator for the <see cref="PublicChangePasswordCommand" /> ensuring proper password change data format.
/// </summary>
/// <remarks>
/// Validates old password and new password according to security requirements:
/// - OldPassword: Required for current password verification
/// - NewPassword: Strong password with mixed cases, numbers, minimum length
/// - NewPassword: Must be different from old password (business logic validation)
/// </remarks>
public class PublicChangePasswordValidator : AbstractValidator<PublicChangePasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicChangePasswordValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicChangePasswordValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.OldPassword).ValidOldPassword(currentPasswordRequired: msg.CurrentPasswordRequired());
        RuleFor(x => x.NewPassword)
            .ValidPassword(
                passwordRequired: msg.PasswordRequired(),
                passwordTooShort: msg.PasswordTooShort("New password", UserConstants.MinPasswordLength),
                passwordComplexity: msg.PasswordComplexity("New password")
            );
    }
}
