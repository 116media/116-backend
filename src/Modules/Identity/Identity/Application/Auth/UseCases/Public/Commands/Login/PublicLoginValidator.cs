using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.Login;

/// <summary>
/// Validator for the <see cref="PublicLoginCommand" /> ensuring proper user credential format.
/// </summary>
/// <remarks>
/// Validates credentials and password presence for user login attempts.
/// </remarks>
public class PublicLoginValidator : AbstractValidator<PublicLoginCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicLoginValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public PublicLoginValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Credentials).ValidCredentials(i18n.User.Validation);
        RuleFor(x => x.Password).ValidPassword(i18n.User.Validation, isStrong: false);
    }
}
