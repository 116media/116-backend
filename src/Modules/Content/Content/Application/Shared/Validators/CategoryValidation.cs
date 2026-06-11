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
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="nameRequired">Error message used when the name is empty.</param>
    /// <param name="nameTooLong">Error message used when the name exceeds the maximum length.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCategoryName<T>(
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
                .MaximumLength(maximumLength: ContentConstants.MaxCategoryNameLength)
                .WithMessage(nameTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxCategoryNameLength)
            .WithMessage(nameTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates category slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="slugRequired">Error message used when the slug is empty.</param>
    /// <param name="slugTooLong">Error message used when the slug exceeds the maximum length.</param>
    /// <param name="slugInvalidFormat">Error message used when the slug does not match the expected format.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCategorySlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string slugRequired,
        string slugTooLong,
        string slugInvalidFormat,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(slugRequired)
                .MaximumLength(maximumLength: ContentConstants.MaxCategorySlugLength)
                .WithMessage(slugTooLong)
                .Matches(SlugRegex())
                .WithMessage(slugInvalidFormat);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxCategorySlugLength)
            .WithMessage(slugTooLong)
            .Matches(SlugRegex())
            .WithMessage(slugInvalidFormat)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates category description with required and length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="descriptionRequired">Error message used when the description is empty.</param>
    /// <param name="descriptionTooLong">Error message used when the description exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCategoryDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string descriptionRequired,
        string descriptionTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(descriptionRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxCategoryDescriptionLength)
            .WithMessage(descriptionTooLong);
    }

    /// <summary>
    /// Validates category pricing price ensuring it is zero or a positive value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the price property.</param>
    /// <param name="priceMustBeNonNegative">Error message used when the price is negative.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, decimal> ValidCategoryPriceUsd<T>(
        this IRuleBuilder<T, decimal> ruleBuilder,
        string priceMustBeNonNegative
    )
    {
        return ruleBuilder.GreaterThanOrEqualTo(valueToCompare: 0).WithMessage(priceMustBeNonNegative);
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
