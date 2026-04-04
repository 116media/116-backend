using _116.Identity.Application.Auth.Validators;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.Login;

/// <summary>
/// Validator for the <see cref="PublicLoginCommand" /> ensuring proper user credential format.
/// </summary>
public class PublicLoginValidator : AbstractValidator<PublicLoginCommand>
{
    /// <summary>
    /// Configure validation rules for public user authentication.
    /// </summary>
    /// <remarks>
    /// Validates credentials and password presence for user login attempts.
    /// </remarks>
    public PublicLoginValidator()
    {
        RuleFor(x => x.Credentials).ValidCredentials();
        RuleFor(x => x.Password).ValidPassword(isStrong: false);
    }
}
