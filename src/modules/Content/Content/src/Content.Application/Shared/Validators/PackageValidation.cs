using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">The package error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPackageName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        PackageErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.NameRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxPackageNameLength)
            .WithMessage(i18n.NameTooLong(ContentConstants.MaxPackageNameLength));
    }

    /// <summary>
    /// Validates package description with required and length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="i18n">The package error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPackageDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        PackageErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.DescriptionRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxPackageDescriptionLength)
            .WithMessage(i18n.DescriptionTooLong(ContentConstants.MaxPackageDescriptionLength));
    }

    /// <summary>
    /// Validates slot quantity ensuring it is greater than zero.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slot quantity property.</param>
    /// <param name="i18n">The package error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, int> ValidSlotQuantity<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        PackageErrorMessage i18n
    )
    {
        return ruleBuilder.GreaterThan(valueToCompare: 0).WithMessage(i18n.SlotQuantityMustBePositive());
    }
}
