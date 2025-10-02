using FluentValidation;
using _116.Auth.Application.Shared.Validators;

namespace _116.Auth.Application.Public.UseCases.Commands.SignUp;

/// <summary>
/// Validator for the <see cref="PublicSignUpCommand"/> ensuring proper user registration data format.
/// </summary>
/// <remarks>
/// Validates username, email, and password according to security and format requirements:
/// - Username: Alphanumeric with spaces and hyphens, minimum 3 characters
/// - Email: Valid email format
/// - Password: Strong password with mixed cases, numbers, minimum 6 characters
/// </remarks>
public partial class PublicSignUpValidator : AbstractValidator<PublicSignUpCommand>
{
    /// <summary>
    /// Configure validation rules for public user registration.
    /// </summary>
    public PublicSignUpValidator()
    {
        // Email validation
        RuleFor(x => x.Email).EmailValidation();

        // Username validation - alphanumeric with spaces and hyphens, min 3 chars
        RuleFor(x => x.UserName).UsernameValidation();

        // Password validation - strong password requirements
        RuleFor(x => x.Password).PasswordValidation();
    }

}
