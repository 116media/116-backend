using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">The promotion level error message provider.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPromotionLevelName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        PromotionLevelErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.NameRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxPromotionLevelNameLength)
                .WithMessage(i18n.NameTooLong(ContentConstants.MaxPromotionLevelNameLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxPromotionLevelNameLength)
            .WithMessage(i18n.NameTooLong(ContentConstants.MaxPromotionLevelNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates promotion duration ensuring it is a positive number of days.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the duration days property.</param>
    /// <param name="i18n">The promotion level error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, int> ValidDurationDays<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        PromotionLevelErrorMessage i18n
    )
    {
        return ruleBuilder.GreaterThan(valueToCompare: 0).WithMessage(i18n.DurationMustBePositive());
    }

    /// <summary>
    /// Validates promotion price ensuring it is zero or a positive value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the price property.</param>
    /// <param name="i18n">The promotion level error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, decimal> ValidPriceUsd<T>(
        this IRuleBuilder<T, decimal> ruleBuilder,
        PromotionLevelErrorMessage i18n
    )
    {
        return ruleBuilder.GreaterThanOrEqualTo(valueToCompare: 0).WithMessage(i18n.PriceMustBeNonNegative());
    }

    /// <summary>
    /// Validates that the spot priority, when provided, is an integer between 1 and 3 (inclusive).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the spot priority property.</param>
    /// <param name="i18n">The promotion level error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, int?> ValidSpotPriority<T>(
        this IRuleBuilder<T, int?> ruleBuilder,
        PromotionLevelErrorMessage i18n
    )
    {
        return ruleBuilder.InclusiveBetween(from: 1, to: 3).WithMessage(i18n.InvalidSpotPriority());
    }
}
