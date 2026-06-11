using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;

/// <summary>
/// Validator for the <see cref="PublicSetPasswordCommand" /> ensuring proper password format.
/// </summary>
/// <remarks>
/// Validates password according to security requirements:
/// - Password: Strong password with mixed cases, numbers, minimum length
/// </remarks>
public class PublicSetPasswordValidator : AbstractValidator<PublicSetPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicSetPasswordValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicSetPasswordValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Password)
            .ValidPassword(
                passwordRequired: msg.PasswordRequired(),
                passwordTooShort: msg.PasswordTooShort("Password", UserConstants.MinPasswordLength),
                passwordComplexity: msg.PasswordComplexity("Password")
            );
    }
}
