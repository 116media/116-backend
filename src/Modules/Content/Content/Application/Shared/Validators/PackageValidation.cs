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
    public static IRuleBuilderOptions<T, string?> ValidPackageName<T>(this IRuleBuilderInitial<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Package name is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxPackageNameLength)
            .WithMessage($"Package name must not exceed {ContentConstants.MaxPackageNameLength} characters.");
    }

    /// <summary>
    /// Validates package description with required and length constraints.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPackageDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Package description is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxPackageDescriptionLength)
            .WithMessage(
                $"Package description must not exceed {ContentConstants.MaxPackageDescriptionLength} characters."
            );
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
