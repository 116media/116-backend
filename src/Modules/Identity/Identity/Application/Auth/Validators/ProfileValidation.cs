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
    /// <param name="countryNameTooLong">Error message when country name exceeds maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCountryName<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string countryNameTooLong
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: UserConstants.MaxCountryNameLength)
            .WithMessage(countryNameTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "CountryName")));
    }

    /// <summary>
    /// Validates country ISO code (2-3 uppercase letters).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country ISO code property.</param>
    /// <param name="countryIsoCodeTooLong">Error message when country ISO code exceeds maximum length.</param>
    /// <param name="countryIsoCodeInvalid">Error message when country ISO code format is invalid.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCountryIsoCode<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string countryIsoCodeTooLong,
        string countryIsoCodeInvalid
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: UserConstants.MaxCountryIsoCodeLength)
            .WithMessage(countryIsoCodeTooLong)
            .Matches("^[A-Z]{2,3}$")
            .WithMessage(countryIsoCodeInvalid)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "CountryIsoCode")));
    }

    /// <summary>
    /// Validates country dial code (+ followed by 1-4 digits).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country dial code property.</param>
    /// <param name="countryDialCodeTooLong">Error message when country dial code exceeds maximum length.</param>
    /// <param name="countryDialCodeInvalid">Error message when country dial code format is invalid.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCountryDialCode<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string countryDialCodeTooLong,
        string countryDialCodeInvalid
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: UserConstants.MaxCountryDialCodeLength)
            .WithMessage(countryDialCodeTooLong)
            .Matches(@"^\+\d{1,4}$")
            .WithMessage(countryDialCodeInvalid)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "CountryDialCode")));
    }

    /// <summary>
    /// Validates partial phone number with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the partial phone number property.</param>
    /// <param name="partialPhoneNumberTooLong">Error message when partial phone number exceeds maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPartialPhoneNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string partialPhoneNumberTooLong
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: UserConstants.MaxPartialPhoneNumberLength)
            .WithMessage(partialPhoneNumberTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "PartialPhoneNumber")));
    }
}
