using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="msg">The error message provider for pricing tier validation messages.</param>
    public static void ValidPricingTierId<T>(this IRuleBuilder<T, Guid> ruleBuilder, PricingTierErrorMessage msg)
    {
        ruleBuilder.NotEmpty().WithMessage(msg.NameRequired());
    }

    /// <summary>
    /// Validates pricing tier name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="msg">The error message provider for pricing tier validation messages.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPricingTierName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        PricingTierErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.NameRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxPricingTierNameLength)
                .WithMessage(msg.NameTooLong(ContentConstants.MaxPricingTierNameLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxPricingTierNameLength)
            .WithMessage(msg.NameTooLong(ContentConstants.MaxPricingTierNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates pricing tier description with required and length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="msg">The error message provider for pricing tier validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPricingTierDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        PricingTierErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.DescriptionRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxPricingTierDescriptionLength)
            .WithMessage(msg.DescriptionTooLong(ContentConstants.MaxPricingTierDescriptionLength));
    }
}
