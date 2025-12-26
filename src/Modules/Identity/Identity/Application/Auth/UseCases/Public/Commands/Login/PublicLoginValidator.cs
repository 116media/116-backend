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
        RuleFor(x => x.Credentials)
            .NotEmpty().WithMessage("Email or username is required.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty.");
    }
}
