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
    /// <param name="nameRequired">Error message used when the ID is empty.</param>
    public static void ValidPricingTierId<T>(this IRuleBuilder<T, Guid> ruleBuilder, string nameRequired)
    {
        ruleBuilder.NotEmpty().WithMessage(nameRequired);
    }

    /// <summary>
    /// Validates pricing tier name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="nameRequired">Error message used when the name is empty.</param>
    /// <param name="nameTooLong">Error message used when the name exceeds the maximum length.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPricingTierName<T>(
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
                .MaximumLength(maximumLength: ContentConstants.MaxPricingTierNameLength)
                .WithMessage(nameTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxPricingTierNameLength)
            .WithMessage(nameTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates pricing tier description with required and length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="descriptionRequired">Error message used when the description is empty.</param>
    /// <param name="descriptionTooLong">Error message used when the description exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPricingTierDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string descriptionRequired,
        string descriptionTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(descriptionRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxPricingTierDescriptionLength)
            .WithMessage(descriptionTooLong);
    }
}
