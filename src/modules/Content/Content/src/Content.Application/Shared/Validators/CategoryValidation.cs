using System.Text.RegularExpressions;
using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">The category error message provider.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCategoryName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        CategoryErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.NameRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxCategoryNameLength)
                .WithMessage(i18n.NameTooLong(ContentConstants.MaxCategoryNameLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxCategoryNameLength)
            .WithMessage(i18n.NameTooLong(ContentConstants.MaxCategoryNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates category slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="i18n">The category error message provider.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCategorySlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        CategoryErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.SlugRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxCategorySlugLength)
                .WithMessage(i18n.SlugTooLong(ContentConstants.MaxCategorySlugLength))
                .Matches(SlugRegex())
                .WithMessage(i18n.SlugInvalidFormat());
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxCategorySlugLength)
            .WithMessage(i18n.SlugTooLong(ContentConstants.MaxCategorySlugLength))
            .Matches(SlugRegex())
            .WithMessage(i18n.SlugInvalidFormat())
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates category description with required and length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="i18n">The category error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCategoryDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        CategoryErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.DescriptionRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxCategoryDescriptionLength)
            .WithMessage(i18n.DescriptionTooLong(ContentConstants.MaxCategoryDescriptionLength));
    }

    /// <summary>
    /// Validates category pricing price ensuring it is zero or a positive value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the price property.</param>
    /// <param name="i18n">The category error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, decimal> ValidCategoryPriceUsd<T>(
        this IRuleBuilder<T, decimal> ruleBuilder,
        CategoryErrorMessage i18n
    )
    {
        return ruleBuilder.GreaterThanOrEqualTo(valueToCompare: 0).WithMessage(i18n.PriceMustBeNonNegative());
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
