using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for pricing tier field validation (id, name, description).
/// </summary>
public static class PricingTierValidation
{
    /// <summary>
    /// Validates that a pricing tier ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the ID property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, Guid> ValidPricingTierId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder.NotEmpty().WithMessage("Pricing tier ID is required.");
    }

    /// <summary>
    /// Validates pricing tier name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPricingTierName<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty()
                .WithMessage("Pricing tier name is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxPricingTierNameLength)
                .WithMessage(
                    $"Pricing tier name must not exceed {ContentConstants.MaxPricingTierNameLength} characters."
                );
        }
        else
        {
            builder = ruleBuilder
                .MaximumLength(maximumLength: ContentConstants.MaxPricingTierNameLength)
                .WithMessage(
                    $"Pricing tier name must not exceed {ContentConstants.MaxPricingTierNameLength} characters."
                )
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
        }

        return builder;
    }

    /// <summary>
    /// Validates pricing tier description with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="isRequired">Whether the description is required (default: false).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPricingTierDescription<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = false
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty()
                .WithMessage("Pricing tier description is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxPricingTierDescriptionLength)
                .WithMessage(
                    $"Pricing tier description must not exceed {ContentConstants.MaxPricingTierDescriptionLength} characters."
                );
        }
        else
        {
            builder = ruleBuilder
                .MaximumLength(maximumLength: ContentConstants.MaxPricingTierDescriptionLength)
                .WithMessage(
                    $"Pricing tier description must not exceed {ContentConstants.MaxPricingTierDescriptionLength} characters."
                )
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Description")));
        }

        return builder;
    }
}
