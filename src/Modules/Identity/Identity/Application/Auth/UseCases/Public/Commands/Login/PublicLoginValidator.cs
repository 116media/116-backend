using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicLoginValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Credentials).ValidCredentials(msg);
        RuleFor(x => x.Password).ValidPassword(msg, isStrong: false);
    }
}
