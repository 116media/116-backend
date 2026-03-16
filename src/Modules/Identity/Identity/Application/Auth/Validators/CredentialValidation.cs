using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;
using FluentValidation;

namespace _116.Identity.Application.Auth.Validators;

/// <summary>
/// FluentValidation extensions for user credential validation (email, password, username).
/// </summary>
public static partial class CredentialValidation
{
    /// <summary>
    /// Validates email with format and length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the email property.</param>
    /// <param name="isRequired">Whether the email is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidEmail<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email is required")
                .MaximumLength(maximumLength: UserConstants.MaxEmailLength)
                .WithMessage($"Email cannot exceed {UserConstants.MaxEmailLength} characters")
                .EmailAddress()
                .WithMessage("Invalid email format");
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: UserConstants.MaxEmailLength)
            .WithMessage($"Email cannot exceed {UserConstants.MaxEmailLength} characters")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Email")));
    }

    /// <summary>
    /// Validates password with strength requirements (lowercase, uppercase, number, minimum length).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the password property.</param>
    /// <param name="fieldName">The name of the field for error messages (default: "Password").</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        string fieldName = "Password"
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage($"{fieldName} is required")
            .MinimumLength(minimumLength: UserConstants.MinPasswordLength)
            .WithMessage($"{fieldName} must be at least {UserConstants.MinPasswordLength} characters long")
            .Matches(PasswordRegex())
            .WithMessage(
                $"{fieldName} must contain at least one lowercase letter, one uppercase letter, and one number"
            );
    }

    /// <summary>
    /// Validates username with alphanumeric, space, and hyphen constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the username property.</param>
    /// <param name="isRequired">Whether the username is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidUsername<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Username is required")
                .MinimumLength(minimumLength: UserConstants.MinUserNameLength)
                .WithMessage($"Username must be at least {UserConstants.MinUserNameLength} characters long")
                .MaximumLength(maximumLength: UserConstants.MaxUserNameLength)
                .WithMessage($"Username cannot exceed {UserConstants.MaxUserNameLength} characters")
                .Matches(UsernameRegex())
                .WithMessage("Username can only contain letters, numbers, spaces, and hyphens");
        }
        else
        {
            builder = ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .MinimumLength(minimumLength: UserConstants.MinUserNameLength)
                .WithMessage($"Username must be at least {UserConstants.MinUserNameLength} characters long")
                .MaximumLength(maximumLength: UserConstants.MaxUserNameLength)
                .WithMessage($"Username cannot exceed {UserConstants.MaxUserNameLength} characters")
                .Matches(UsernameRegex())
                .WithMessage("Username can only contain letters, numbers, spaces, and hyphens")
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "UserName")));
        }

        return builder;
    }

    /// <summary>
    /// Validates old password (required for verification during password changes).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the old password property.</param>
    /// <param name="fieldName">The name of the field for error messages (default: "Current password").</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidOldPassword<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string fieldName = "Current password"
    )
    {
        return ruleBuilder.NotEmpty().WithMessage($"{fieldName} is required");
    }

    /// <summary>
    /// Generated regex for password validation - at least one lowercase, one uppercase, and one number.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])")]
    private static partial Regex PasswordRegex();

    /// <summary>
    /// Generated regex for username validation - alphanumeric, spaces, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9\-\s]+$")]
    private static partial Regex UsernameRegex();
}
