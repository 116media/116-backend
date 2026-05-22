using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;

/// <summary>
/// Validator for the <see cref="PublicSignUpCommand" /> ensuring proper user registration data format.
/// </summary>
/// <remarks>
/// Validates username, email, and password according to security and format requirements:
/// - Username: Alphanumeric with spaces and hyphens, minimum 3 characters
/// - Email: Valid email format
/// - Password: Strong password with mixed cases, numbers, minimum 6 characters
/// </remarks>
public class PublicSignUpValidator : AbstractValidator<PublicSignUpCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicSignUpValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicSignUpValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Email)
            .ValidEmail(
                emailRequired: msg.EmailRequired(),
                emailTooLong: msg.EmailTooLong(UserConstants.MaxEmailLength),
                invalidEmailFormat: msg.InvalidEmailFormatMsg()
            );
        RuleFor(x => x.UserName)
            .ValidUsername(
                usernameRequired: msg.UsernameRequired(),
                usernameTooShort: msg.UsernameTooShort(UserConstants.MinUserNameLength),
                usernameTooLong: msg.UsernameTooLong(UserConstants.MaxUserNameLength),
                usernameInvalidChars: msg.UsernameInvalidChars()
            );
        RuleFor(x => x.Password)
            .ValidPassword(
                passwordRequired: msg.PasswordRequired(),
                passwordTooShort: msg.PasswordTooShort("Password", UserConstants.MinPasswordLength),
                passwordComplexity: msg.PasswordComplexity("Password")
            );
    }
}
