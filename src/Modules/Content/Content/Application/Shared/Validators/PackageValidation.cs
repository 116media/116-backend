using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for package and package slot field validation.
/// </summary>
public static class PackageValidation
{
    /// <summary>
    /// Validates that a package ID is not empty.
    /// </summary>
    public static void ValidPackageId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        ruleBuilder.NotEmpty().WithMessage("Package ID is required.");
    }

    /// <summary>
    /// Validates package name with length constraints.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPackageName<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Package name is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxPackageNameLength)
            .WithMessage($"Package name must not exceed {ContentConstants.MaxPackageNameLength} characters.");
    }

    /// <summary>
    /// Validates optional package description with length constraints.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPackageDescription<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxPackageDescriptionLength)
            .WithMessage(
                $"Package description must not exceed {ContentConstants.MaxPackageDescriptionLength} characters."
            )
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Description")));
    }

    /// <summary>
    /// Validates package flat price ensuring it is zero or a positive value.
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> ValidFlatPriceUsd<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(valueToCompare: 0)
            .WithMessage("Package price must be zero or greater.");
    }

    /// <summary>
    /// Validates slot quantity ensuring it is greater than zero.
    /// </summary>
    public static IRuleBuilderOptions<T, int> ValidSlotQuantity<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder.GreaterThan(valueToCompare: 0).WithMessage("Slot quantity must be greater than zero.");
    }
}
