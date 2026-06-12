using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <param name="isRequired">Whether the email is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidEmail<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ValidationErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.EmailRequired())
                .MaximumLength(maximumLength: UserConstants.MaxEmailLength)
                .WithMessage(i18n.EmailTooLong(UserConstants.MaxEmailLength))
                .EmailAddress()
                .WithMessage(i18n.InvalidEmailFormatMsg());
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: UserConstants.MaxEmailLength)
            .WithMessage(i18n.EmailTooLong(UserConstants.MaxEmailLength))
            .EmailAddress()
            .WithMessage(i18n.InvalidEmailFormatMsg())
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Email")));
    }

    /// <summary>
    /// Validates password with optional complexity requirements (lowercase, uppercase, number, minimum length).
    /// When <paramref name="isStrong"/> is false, only presence is validated (used for login flows).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the password property.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <param name="fieldName">The display name of the field (e.g., "Password", "New password").</param>
    /// <param name="isStrong">Whether to enforce complexity rules (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        ValidationErrorMessage i18n,
        string fieldName = "Password",
        bool isStrong = true
    )
    {
        IRuleBuilderOptions<T, string> builder = ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.PasswordRequired());

        if (!isStrong)
        {
            return builder;
        }

        return builder
            .MinimumLength(minimumLength: UserConstants.MinPasswordLength)
            .WithMessage(i18n.PasswordTooShort(fieldName, UserConstants.MinPasswordLength))
            .Matches(PasswordRegex())
            .WithMessage(i18n.PasswordComplexity(fieldName));
    }

    /// <summary>
    /// Validates username with alphanumeric, space, and hyphen constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the username property.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <param name="isRequired">Whether the username is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidUsername<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ValidationErrorMessage i18n,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.UsernameRequired())
                .MinimumLength(minimumLength: UserConstants.MinUserNameLength)
                .WithMessage(i18n.UsernameTooShort(UserConstants.MinUserNameLength))
                .MaximumLength(maximumLength: UserConstants.MaxUserNameLength)
                .WithMessage(i18n.UsernameTooLong(UserConstants.MaxUserNameLength))
                .Matches(UsernameRegex())
                .WithMessage(i18n.UsernameInvalidChars());
        }
        else
        {
            builder = ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .MinimumLength(minimumLength: UserConstants.MinUserNameLength)
                .WithMessage(i18n.UsernameTooShort(UserConstants.MinUserNameLength))
                .MaximumLength(maximumLength: UserConstants.MaxUserNameLength)
                .WithMessage(i18n.UsernameTooLong(UserConstants.MaxUserNameLength))
                .Matches(UsernameRegex())
                .WithMessage(i18n.UsernameInvalidChars())
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "UserName")));
        }

        return builder;
    }

    /// <summary>
    /// Validates old password (required for verification during password changes).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the old password property.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidOldPassword<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        ValidationErrorMessage i18n
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(i18n.CurrentPasswordRequired());
    }

    /// <summary>
    /// Validates login credentials presence (email or username, no format checks).
    /// Used for login flows where either email or username is accepted.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the credentials property.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidCredentials<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        ValidationErrorMessage i18n
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(i18n.EmailOrUsernameRequired());
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
