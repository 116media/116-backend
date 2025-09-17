using FluentValidation;
using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;

namespace _116.User.Application.Public.UseCases.Commands.SignUp;

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
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        // Username validation - alphanumeric with spaces and hyphens, min 3 chars
        RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(UserConstants.MinUserNameLength)
            .WithMessage($"Username must be at least {UserConstants.MinUserNameLength} characters long")
            .MaximumLength(UserConstants.MaxUserNameLength)
            .WithMessage($"Username cannot exceed {UserConstants.MaxUserNameLength} characters")
            .Matches(UsernameRegex())
            .WithMessage("Username can only contain letters, numbers, spaces, and hyphens");

        // Password validation - strong password requirements
        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(UserConstants.MinPasswordLength)
            .WithMessage($"Password must be at least {UserConstants.MinPasswordLength} characters long")
            .Matches(PasswordRegex())
            .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one number");
    }

    /// <summary>
    /// Generated regex for username validation - alphanumeric, spaces, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9\-\s]+$")]
    private static partial Regex UsernameRegex();

    /// <summary>
    /// Generated regex for password validation - at least one lowercase, one uppercase, and one number.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])")]
    private static partial Regex PasswordRegex();
}
