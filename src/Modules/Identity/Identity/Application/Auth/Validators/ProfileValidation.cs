using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// <param name="msg">The validation error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCountryName<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        ValidationErrorMessage msg
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: UserConstants.MaxCountryNameLength)
            .WithMessage(msg.CountryNameTooLong(UserConstants.MaxCountryNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "CountryName")));
    }

    /// <summary>
    /// Validates country ISO code (2-3 uppercase letters).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country ISO code property.</param>
    /// <param name="msg">The validation error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCountryIsoCode<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ValidationErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: UserConstants.MaxCountryIsoCodeLength)
            .WithMessage(msg.CountryIsoCodeTooLong(UserConstants.MaxCountryIsoCodeLength))
            .Matches("^[A-Z]{2,3}$")
            .WithMessage(msg.CountryIsoCodeInvalid())
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "CountryIsoCode")));
    }

    /// <summary>
    /// Validates country dial code (+ followed by 1-4 digits).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the country dial code property.</param>
    /// <param name="msg">The validation error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCountryDialCode<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ValidationErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: UserConstants.MaxCountryDialCodeLength)
            .WithMessage(msg.CountryDialCodeTooLong(UserConstants.MaxCountryDialCodeLength))
            .Matches(@"^\+\d{1,4}$")
            .WithMessage(msg.CountryDialCodeInvalid(UserConstants.MaxCountryDialCodeLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "CountryDialCode")));
    }

    /// <summary>
    /// Validates partial phone number with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the partial phone number property.</param>
    /// <param name="msg">The validation error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPartialPhoneNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        ValidationErrorMessage msg
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: UserConstants.MaxPartialPhoneNumberLength)
            .WithMessage(msg.PartialPhoneNumberTooLong(UserConstants.MaxPartialPhoneNumberLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "PartialPhoneNumber")));
    }
}
