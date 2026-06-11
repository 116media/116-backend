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
    /// <param name="emailRequired">Error message when email is missing.</param>
    /// <param name="emailTooLong">Error message when email exceeds maximum length.</param>
    /// <param name="invalidEmailFormat">Error message when email format is invalid.</param>
    /// <param name="isRequired">Whether the email is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidEmail<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string emailRequired,
        string emailTooLong,
        string invalidEmailFormat,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(emailRequired)
                .MaximumLength(maximumLength: UserConstants.MaxEmailLength)
                .WithMessage(emailTooLong)
                .EmailAddress()
                .WithMessage(invalidEmailFormat);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: UserConstants.MaxEmailLength)
            .WithMessage(emailTooLong)
            .EmailAddress()
            .WithMessage(invalidEmailFormat)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Email")));
    }

    /// <summary>
    /// Validates password with optional complexity requirements (lowercase, uppercase, number, minimum length).
    /// When <paramref name="isStrong"/> is false, only presence is validated (used for login flows).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the password property.</param>
    /// <param name="passwordRequired">Error message when password is missing.</param>
    /// <param name="passwordTooShort">Error message when password is below minimum length (only used when isStrong is true).</param>
    /// <param name="passwordComplexity">Error message when password does not meet complexity rules (only used when isStrong is true).</param>
    /// <param name="isStrong">Whether to enforce complexity rules (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        string passwordRequired,
        string passwordTooShort = "",
        string passwordComplexity = "",
        bool isStrong = true
    )
    {
        IRuleBuilderOptions<T, string> builder = ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(passwordRequired);

        if (!isStrong)
        {
            return builder;
        }

        return builder
            .MinimumLength(minimumLength: UserConstants.MinPasswordLength)
            .WithMessage(passwordTooShort)
            .Matches(PasswordRegex())
            .WithMessage(passwordComplexity);
    }

    /// <summary>
    /// Validates username with alphanumeric, space, and hyphen constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the username property.</param>
    /// <param name="usernameRequired">Error message when username is missing.</param>
    /// <param name="usernameTooShort">Error message when username is below minimum length.</param>
    /// <param name="usernameTooLong">Error message when username exceeds maximum length.</param>
    /// <param name="usernameInvalidChars">Error message when username contains invalid characters.</param>
    /// <param name="isRequired">Whether the username is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidUsername<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string usernameRequired,
        string usernameTooShort,
        string usernameTooLong,
        string usernameInvalidChars,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(usernameRequired)
                .MinimumLength(minimumLength: UserConstants.MinUserNameLength)
                .WithMessage(usernameTooShort)
                .MaximumLength(maximumLength: UserConstants.MaxUserNameLength)
                .WithMessage(usernameTooLong)
                .Matches(UsernameRegex())
                .WithMessage(usernameInvalidChars);
        }
        else
        {
            builder = ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .MinimumLength(minimumLength: UserConstants.MinUserNameLength)
                .WithMessage(usernameTooShort)
                .MaximumLength(maximumLength: UserConstants.MaxUserNameLength)
                .WithMessage(usernameTooLong)
                .Matches(UsernameRegex())
                .WithMessage(usernameInvalidChars)
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "UserName")));
        }

        return builder;
    }

    /// <summary>
    /// Validates old password (required for verification during password changes).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the old password property.</param>
    /// <param name="currentPasswordRequired">Error message when the current password is missing.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidOldPassword<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string currentPasswordRequired
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(currentPasswordRequired);
    }

    /// <summary>
    /// Validates login credentials presence (email or username, no format checks).
    /// Used for login flows where either email or username is accepted.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the credentials property.</param>
    /// <param name="emailOrUsernameRequired">Error message when email or username is missing.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidCredentials<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        string emailOrUsernameRequired
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(emailOrUsernameRequired);
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
