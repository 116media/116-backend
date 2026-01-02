using _116.Identity.Application.Auth.Validators;

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
    /// Configure validation rules for password change.
    /// </summary>
    public PublicChangePasswordValidator()
    {
        RuleFor(x => x.OldPassword).ValidOldPassword();
        RuleFor(x => x.NewPassword).ValidPassword("New password");
    }
}
