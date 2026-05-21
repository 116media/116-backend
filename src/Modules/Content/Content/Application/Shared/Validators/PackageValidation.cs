using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for package and package slot field validation.
/// </summary>
public static class PackageValidation
{
    /// <summary>
    /// Validates package name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="nameRequired">Error message used when the name is empty.</param>
    /// <param name="nameTooLong">Error message used when the name exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPackageName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string nameRequired,
        string nameTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(nameRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxPackageNameLength)
            .WithMessage(nameTooLong);
    }

    /// <summary>
    /// Validates package description with required and length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="descriptionRequired">Error message used when the description is empty.</param>
    /// <param name="descriptionTooLong">Error message used when the description exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPackageDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string descriptionRequired,
        string descriptionTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(descriptionRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxPackageDescriptionLength)
            .WithMessage(descriptionTooLong);
    }

    /// <summary>
    /// Validates slot quantity ensuring it is greater than zero.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slot quantity property.</param>
    /// <param name="slotQuantityMustBePositive">Error message used when the slot quantity is not positive.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, int> ValidSlotQuantity<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        string slotQuantityMustBePositive
    )
    {
        return ruleBuilder.GreaterThan(valueToCompare: 0).WithMessage(slotQuantityMustBePositive);
    }
}
