using _116.Identity.Application.Auth.Validators;

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
    /// Configure validation rules for public user registration.
    /// </summary>
    public PublicSignUpValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.UserName).ValidUsername();
        RuleFor(x => x.Password).ValidPassword();
    }
}
