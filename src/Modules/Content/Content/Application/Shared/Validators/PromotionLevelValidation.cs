using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for promotion level field validation (id, name, duration, price).
/// </summary>
public static class PromotionLevelValidation
{
    /// <summary>
    /// Validates promotion level name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="nameRequired">Error message used when the name is empty.</param>
    /// <param name="nameTooLong">Error message used when the name exceeds the maximum length.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPromotionLevelName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string nameRequired,
        string nameTooLong,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(nameRequired)
                .MaximumLength(maximumLength: ContentConstants.MaxPromotionLevelNameLength)
                .WithMessage(nameTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxPromotionLevelNameLength)
            .WithMessage(nameTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates promotion duration ensuring it is a positive number of days.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the duration days property.</param>
    /// <param name="durationMustBePositive">Error message used when the duration is not a positive value.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, int> ValidDurationDays<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        string durationMustBePositive
    )
    {
        return ruleBuilder.GreaterThan(valueToCompare: 0).WithMessage(durationMustBePositive);
    }

    /// <summary>
    /// Validates promotion price ensuring it is zero or a positive value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the price property.</param>
    /// <param name="priceMustBeNonNegative">Error message used when the price is negative.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, decimal> ValidPriceUsd<T>(
        this IRuleBuilder<T, decimal> ruleBuilder,
        string priceMustBeNonNegative
    )
    {
        return ruleBuilder.GreaterThanOrEqualTo(valueToCompare: 0).WithMessage(priceMustBeNonNegative);
    }

    /// <summary>
    /// Validates that the spot priority, when provided, is an integer between 1 and 3 (inclusive).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the spot priority property.</param>
    /// <param name="invalidSpotPriority">Error message used when the spot priority is outside the allowed range.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, int?> ValidSpotPriority<T>(
        this IRuleBuilder<T, int?> ruleBuilder,
        string invalidSpotPriority
    )
    {
        return ruleBuilder.InclusiveBetween(from: 1, to: 3).WithMessage(invalidSpotPriority);
    }
}
