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
    /// <param name="msg">The error message provider for category validation messages.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCategoryName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        CategoryErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.NameRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxCategoryNameLength)
                .WithMessage(msg.NameTooLong(ContentConstants.MaxCategoryNameLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxCategoryNameLength)
            .WithMessage(msg.NameTooLong(ContentConstants.MaxCategoryNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates category slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="msg">The error message provider for category validation messages.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCategorySlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        CategoryErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.SlugRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxCategorySlugLength)
                .WithMessage(msg.SlugTooLong(ContentConstants.MaxCategorySlugLength))
                .Matches(SlugRegex())
                .WithMessage(msg.SlugInvalidFormat());
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxCategorySlugLength)
            .WithMessage(msg.SlugTooLong(ContentConstants.MaxCategorySlugLength))
            .Matches(SlugRegex())
            .WithMessage(msg.SlugInvalidFormat())
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates category description with required and length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="msg">The error message provider for category validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCategoryDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        CategoryErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.DescriptionRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxCategoryDescriptionLength)
            .WithMessage(msg.DescriptionTooLong(ContentConstants.MaxCategoryDescriptionLength));
    }

    /// <summary>
    /// Validates category pricing price ensuring it is zero or a positive value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the price property.</param>
    /// <param name="msg">The error message provider for category validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, decimal> ValidCategoryPriceUsd<T>(
        this IRuleBuilder<T, decimal> ruleBuilder,
        CategoryErrorMessage msg
    )
    {
        return ruleBuilder.GreaterThanOrEqualTo(valueToCompare: 0).WithMessage(msg.PriceMustBeNonNegative());
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
