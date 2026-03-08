using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for promotion level field validation (id, name, duration, price).
/// </summary>
public static class PromotionLevelValidation
{
    /// <summary>
    /// Validates that a promotion level ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the ID property.</param>
    /// <returns>The configured rule builder.</returns>
    public static void ValidPromotionLevelId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        ruleBuilder.NotEmpty().WithMessage("Promotion level ID is required.");
    }

    /// <summary>
    /// Validates promotion level name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPromotionLevelName<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty()
                .WithMessage("Promotion level name is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxPromotionLevelNameLength)
                .WithMessage(
                    $"Promotion level name must not exceed {ContentConstants.MaxPromotionLevelNameLength} characters."
                );
        }
        else
        {
            builder = ruleBuilder
                .MaximumLength(maximumLength: ContentConstants.MaxPromotionLevelNameLength)
                .WithMessage(
                    $"Promotion level name must not exceed {ContentConstants.MaxPromotionLevelNameLength} characters."
                )
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
        }

        return builder;
    }

    /// <summary>
    /// Validates promotion duration ensuring it is a positive number of days.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the duration days property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, int> ValidDurationDays<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(valueToCompare: 0)
            .WithMessage("Promotion level duration must be greater than zero.");
    }

    /// <summary>
    /// Validates promotion price ensuring it is zero or a positive value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the price property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, decimal> ValidPriceUsd<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(valueToCompare: 0)
            .WithMessage("Promotion level price must be zero or greater.");
    }
}
