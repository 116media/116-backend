using System.Reflection;
using FluentValidation;
using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;
using _116.Auth.Domain.Enums;

namespace _116.Auth.Application.Shared.Validators;

/// <summary>
/// Contains common validation rules and helpers for user-related validators.
/// This class provides reusable validation patterns used across multiple validators.
/// </summary>
public static partial class UserValidationRules
{
    /// <summary>
    /// Configures email validation rules.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the email property.</param>
    /// <param name="isRequired">Whether the email is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> EmailValidation<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;

        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(UserConstants.MaxEmailLength)
                .WithMessage($"Email cannot exceed {UserConstants.MaxEmailLength} characters")
                .EmailAddress().WithMessage("Invalid email format");
        }
        else
        {
            builder = ruleBuilder
                .MaximumLength(UserConstants.MaxEmailLength)
                .WithMessage($"Email cannot exceed {UserConstants.MaxEmailLength} characters")
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrWhiteSpace(GetEmailValue(x)));
        }

        return builder;
    }

    /// <summary>
    /// Configures password validation rules with strength requirements.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the password property.</param>
    /// <param name="fieldName">The name of the field for error messages (default: "Password").</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> PasswordValidation<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        string fieldName = "Password"
    )
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage($"{fieldName} is required")
            .MinimumLength(UserConstants.MinPasswordLength)
            .WithMessage($"{fieldName} must be at least {UserConstants.MinPasswordLength} characters long")
            .Matches(PasswordRegex())
            .WithMessage($"{fieldName} must contain at least one lowercase letter, one uppercase letter, and one number");
    }

    /// <summary>
    /// Configures OTP code validation rules.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilderInitial">The rule builder for the OTP code property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> OtpCodeValidation<T>(
        this IRuleBuilderInitial<T, string> ruleBuilderInitial
    )
    {
        return ruleBuilderInitial
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(UserConstants.OtpCodeLength)
            .WithMessage($"Verification code must be exactly {UserConstants.OtpCodeLength} characters long")
            .Matches(OtpCodeRegex())
            .WithMessage("Verification code must contain only numbers");
    }

    /// <summary>
    /// Configures username validation rules.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the username property.</param>
    /// <param name="isRequired">Whether the username is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> UsernameValidation<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;

        if (isRequired)
        {
            builder = ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(UserConstants.MinUserNameLength)
                .WithMessage($"Username must be at least {UserConstants.MinUserNameLength} characters long")
                .MaximumLength(UserConstants.MaxUserNameLength)
                .WithMessage($"Username cannot exceed {UserConstants.MaxUserNameLength} characters")
                .Matches(UsernameRegex())
                .WithMessage("Username can only contain letters, numbers, spaces, and hyphens");
        }
        else
        {
            builder = ruleBuilder
                .Cascade(CascadeMode.Stop)
                .MinimumLength(UserConstants.MinUserNameLength)
                .WithMessage($"Username must be at least {UserConstants.MinUserNameLength} characters long")
                .MaximumLength(UserConstants.MaxUserNameLength)
                .WithMessage($"Username cannot exceed {UserConstants.MaxUserNameLength} characters")
                .Matches(UsernameRegex())
                .WithMessage("Username can only contain letters, numbers, spaces, and hyphens")
                .When(x => !string.IsNullOrWhiteSpace(GetUsernameValue(x)));
        }

        return builder;
    }

    /// <summary>
    /// Configures country name validation rules.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country name property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> CountryNameValidation<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .MaximumLength(UserConstants.MaxCountryNameLength)
            .WithMessage($"Country name cannot exceed {UserConstants.MaxCountryNameLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(GetCountryNameValue(x)));
    }

    /// <summary>
    /// Configures country flag URL validation rules.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country flag URL property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> CountryFlagUrlValidation<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .MaximumLength(UserConstants.MaxCountryFlagUrlLength)
            .WithMessage($"Country flag URL cannot exceed {UserConstants.MaxCountryFlagUrlLength} characters")
            .Must(BeValidUrl)
            .WithMessage("Country flag URL must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(GetCountryFlagUrlValue(x)));
    }

    /// <summary>
    /// Configures country ISO code validation rules.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country ISO code property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> CountryIsoCodeValidation<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .MaximumLength(UserConstants.MaxCountryIsoCodeLength)
            .WithMessage($"Country ISO code cannot exceed {UserConstants.MaxCountryIsoCodeLength} characters")
            .Matches("^[A-Z]{2,3}$")
            .WithMessage("Country ISO code must contain only uppercase letters")
            .When(x => !string.IsNullOrWhiteSpace(GetCountryIsoCodeValue(x)));
    }

    /// <summary>
    /// Configures country dial code validation rules.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country dial code property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> CountryDialCodeValidation<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .MaximumLength(UserConstants.MaxCountryDialCodeLength)
            .WithMessage($"Country dial code cannot exceed {UserConstants.MaxCountryDialCodeLength} characters")
            .Matches(@"^\+\d{1,4}$")
            .WithMessage($"Country dial code must start with + followed by 1-{UserConstants.MaxCountryDialCodeLength} digits")
            .When(x => !string.IsNullOrWhiteSpace(GetCountryDialCodeValue(x)));
    }

    /// <summary>
    /// Configures partial phone number validation rules.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the partial phone number property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> PartialPhoneNumberValidation<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .MaximumLength(UserConstants.MaxPartialPhoneNumberLength)
            .WithMessage($"Partial phone number cannot exceed {UserConstants.MaxPartialPhoneNumberLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(GetPartialPhoneNumberValue(x)));
    }

    /// <summary>
    /// Configures OTP purpose validation rules.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the OTP purpose property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> OtpPurposeValidation<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("OTP purpose is required.")
            .Must(purpose => purpose != null && Enum.IsDefined(typeof(OtpPurpose), purpose))
            .WithMessage("Invalid OTP purpose specified.");
    }

    /// <summary>
    /// Configures old password validation rules (required for verification).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the old password property.</param>
    /// <param name="fieldName">The name of the field for error messages (default: "Current password").</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> OldPasswordValidation<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string fieldName = "Current password"
    )
    {
        return ruleBuilder
            .NotEmpty().WithMessage($"{fieldName} is required");
    }

    /// <summary>
    /// Configures cross-field validation to ensure new password is different from old password.
    /// </summary>
    /// <typeparam name="T">The type being validated (must have OldPassword and NewPassword properties).</typeparam>
    /// <param name="ruleBuilder">The rule builder for the command object.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, T> NewPasswordMustBeDifferent<T>(
        this IRuleBuilderInitial<T, T> ruleBuilder
    )
    {
        return ruleBuilder
            .Must(command => GetOldPasswordValue(command) != GetNewPasswordValue(command))
            .WithMessage("New password must be different from current password")
            .WithName("NewPassword");
    }

    /// <summary>
    /// Validates that a URL is in proper format.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid or null/empty, false otherwise.</returns>
    public static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out Uri? result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Gets the Email property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the Email property from.</param>
    /// <returns>The Email property value as a string, or null if not found.</returns>
    private static string? GetEmailValue<T>(T instance)
    {
        return GetPropertyValue(instance, "Email");
    }

    /// <summary>
    /// Gets the UserName property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the UserName property from.</param>
    /// <returns>The UserName property value as a string, or null if not found.</returns>
    private static string? GetUsernameValue<T>(T instance)
    {
        return GetPropertyValue(instance, "UserName");
    }

    /// <summary>
    /// Gets the CountryName property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the CountryName property from.</param>
    /// <returns>The CountryName property value as a string, or null if not found.</returns>
    private static string? GetCountryNameValue<T>(T instance)
    {
        return GetPropertyValue(instance, "CountryName");
    }

    /// <summary>
    /// Gets the CountryFlagUrl property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the CountryFlagUrl property from.</param>
    /// <returns>The CountryFlagUrl property value as a string, or null if not found.</returns>
    private static string? GetCountryFlagUrlValue<T>(T instance)
    {
        return GetPropertyValue(instance, "CountryFlagUrl");
    }

    /// <summary>
    /// Gets the CountryIsoCode property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the CountryIsoCode property from.</param>
    /// <returns>The CountryIsoCode property value as a string, or null if not found.</returns>
    private static string? GetCountryIsoCodeValue<T>(T instance)
    {
        return GetPropertyValue(instance, "CountryIsoCode");
    }

    /// <summary>
    /// Gets the CountryDialCode property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the CountryDialCode property from.</param>
    /// <returns>The CountryDialCode property value as a string, or null if not found.</returns>
    private static string? GetCountryDialCodeValue<T>(T instance)
    {
        return GetPropertyValue(instance, "CountryDialCode");
    }

    /// <summary>
    /// Gets the PartialPhoneNumber property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the PartialPhoneNumber property from.</param>
    /// <returns>The PartialPhoneNumber property value as a string, or null if not found.</returns>
    private static string? GetPartialPhoneNumberValue<T>(T instance)
    {
        return GetPropertyValue(instance, "PartialPhoneNumber");
    }

    /// <summary>
    /// Gets the OldPassword property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the OldPassword property from.</param>
    /// <returns>The OldPassword property value as a string, or null if not found.</returns>
    private static string? GetOldPasswordValue<T>(T instance)
    {
        return GetPropertyValue(instance, "OldPassword");
    }

    /// <summary>
    /// Gets the NewPassword property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the NewPassword property from.</param>
    /// <returns>The NewPassword property value as a string, or null if not found.</returns>
    private static string? GetNewPasswordValue<T>(T instance)
    {
        return GetPropertyValue(instance, "NewPassword");
    }

    /// <summary>
    /// Configures avatar URL validation rules.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the avatar URL property.</param>
    /// <param name="isRequired">Whether the avatar URL is required (default: false).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> AvatarUrlValidation<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = false
    )
    {
        IRuleBuilderOptions<T, string?> builder;

        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty().WithMessage("Avatar URL is required")
                .Must(BeValidUrl).WithMessage("Avatar URL must be a valid URL")
                .MaximumLength(FileConstants.MaxStorageUrlLength)
                .WithMessage($"Avatar URL cannot exceed {FileConstants.MaxStorageUrlLength} characters");
        }
        else
        {
            builder = ruleBuilder
                .Must(BeValidUrl).WithMessage("Avatar URL must be a valid URL")
                .MaximumLength(FileConstants.MaxStorageUrlLength)
                .WithMessage($"Avatar URL cannot exceed {FileConstants.MaxStorageUrlLength} characters")
                .When(x => !string.IsNullOrWhiteSpace(GetAvatarUrlValue(x)));
        }

        return builder;
    }

    /// <summary>
    /// Gets the AvatarUrl property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the AvatarUrl property from.</param>
    /// <returns>The AvatarUrl property value as a string, or null if not found.</returns>
    private static string? GetAvatarUrlValue<T>(T instance)
    {
        return GetPropertyValue(instance, "AvatarUrl");
    }

    /// <summary>
    /// Gets a property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the property value from.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>The property value as a string, or null if the property is not found or cannot be cast to string.</returns>
    private static string? GetPropertyValue<T>(T instance, string propertyName)
    {
        PropertyInfo? property = typeof(T).GetProperty(propertyName);
        return property?.GetValue(instance) as string;
    }

    /// <summary>
    /// Generated regex for password validation - at least one lowercase, one uppercase, and one number.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])")]
    private static partial Regex PasswordRegex();

    /// <summary>
    /// Generated regex for OTP code validation - 6-digit numeric only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex OtpCodeRegex();

    /// <summary>
    /// Generated regex for username validation - alphanumeric, spaces, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9\-\s]+$")]
    private static partial Regex UsernameRegex();
}
