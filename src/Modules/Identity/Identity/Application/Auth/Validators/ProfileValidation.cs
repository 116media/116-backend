using _116.BuildingBlocks.Constants;

using FluentValidation;

namespace _116.Identity.Application.Auth.Validators;

/// <summary>
/// FluentValidation extensions for user profile field validation (country, phone).
/// </summary>
public static class ProfileValidation
{
    /// <summary>
    /// Validates country name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country name property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCountryName<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: UserConstants.MaxCountryNameLength)
            .WithMessage($"Country name cannot exceed {UserConstants.MaxCountryNameLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "CountryName")));
    }

    /// <summary>
    /// Validates country ISO code (2-3 uppercase letters).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country ISO code property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCountryIsoCode<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: UserConstants.MaxCountryIsoCodeLength)
            .WithMessage($"Country ISO code cannot exceed {UserConstants.MaxCountryIsoCodeLength} characters")
            .Matches("^[A-Z]{2,3}$")
            .WithMessage("Country ISO code must contain only uppercase letters")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "CountryIsoCode")));
    }

    /// <summary>
    /// Validates country dial code (+ followed by 1-4 digits).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country dial code property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCountryDialCode<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: UserConstants.MaxCountryDialCodeLength)
            .WithMessage($"Country dial code cannot exceed {UserConstants.MaxCountryDialCodeLength} characters")
            .Matches(@"^\+\d{1,4}$")
            .WithMessage(
                $"Country dial code must start with + followed by 1-{UserConstants.MaxCountryDialCodeLength} digits")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "CountryDialCode")));
    }

    /// <summary>
    /// Validates partial phone number with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the partial phone number property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPartialPhoneNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: UserConstants.MaxPartialPhoneNumberLength)
            .WithMessage($"Partial phone number cannot exceed {UserConstants.MaxPartialPhoneNumberLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "PartialPhoneNumber")));
    }
}
