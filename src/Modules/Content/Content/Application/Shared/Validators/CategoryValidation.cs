using System.Text.RegularExpressions;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for category field validation (id, name, slug, description, price).
/// </summary>
public static partial class CategoryValidation
{
    /// <summary>
    /// Validates category name with length constraints.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCategoryName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Category name is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxCategoryNameLength)
                .WithMessage($"Category name must not exceed {ContentConstants.MaxCategoryNameLength} characters.");
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxCategoryNameLength)
            .WithMessage($"Category name must not exceed {ContentConstants.MaxCategoryNameLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates category slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCategorySlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Category slug is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxCategorySlugLength)
                .WithMessage($"Category slug must not exceed {ContentConstants.MaxCategorySlugLength} characters.")
                .Matches(SlugRegex())
                .WithMessage("Category slug must be lowercase and contain only letters, numbers, and hyphens.");
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxCategorySlugLength)
            .WithMessage($"Category slug must not exceed {ContentConstants.MaxCategorySlugLength} characters.")
            .Matches(SlugRegex())
            .WithMessage("Category slug must be lowercase and contain only letters, numbers, and hyphens.")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates category description with length constraints (optional field).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCategoryDescription<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxCategoryDescriptionLength)
            .WithMessage(
                $"Category description must not exceed {ContentConstants.MaxCategoryDescriptionLength} characters."
            )
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Description")));
    }

    /// <summary>
    /// Validates category pricing price ensuring it is zero or a positive value.
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> ValidCategoryPriceUsd<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(valueToCompare: 0)
            .WithMessage("Category price must be zero or greater.");
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
