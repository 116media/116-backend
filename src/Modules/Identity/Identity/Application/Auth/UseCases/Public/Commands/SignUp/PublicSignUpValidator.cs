using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public PublicSignUpValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Email).ValidEmail(i18n.User.Validation);
        RuleFor(x => x.UserName).ValidUsername(i18n.User.Validation);
        RuleFor(x => x.Password).ValidPassword(i18n.User.Validation);
    }
}
