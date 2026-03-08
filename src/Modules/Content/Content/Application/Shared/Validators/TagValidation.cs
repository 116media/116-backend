using System.Text.RegularExpressions;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for tag field validation (name, slug).
/// </summary>
public static partial class TagValidation
{
    /// <summary>
    /// Validates tag name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidTagName<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty()
                .WithMessage("Tag name is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxTagNameLength)
                .WithMessage($"Tag name must not exceed {ContentConstants.MaxTagNameLength} characters.");
        }
        else
        {
            builder = ruleBuilder
                .MaximumLength(maximumLength: ContentConstants.MaxTagNameLength)
                .WithMessage($"Tag name must not exceed {ContentConstants.MaxTagNameLength} characters.")
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
        }

        return builder;
    }

    /// <summary>
    /// Validates tag slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidTagSlug<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty()
                .WithMessage("Tag slug is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxTagSlugLength)
                .WithMessage($"Tag slug must not exceed {ContentConstants.MaxTagSlugLength} characters.")
                .Matches(SlugRegex())
                .WithMessage("Tag slug must be lowercase and contain only letters, numbers, and hyphens.");
        }
        else
        {
            builder = ruleBuilder
                .MaximumLength(maximumLength: ContentConstants.MaxTagSlugLength)
                .WithMessage($"Tag slug must not exceed {ContentConstants.MaxTagSlugLength} characters.")
                .Matches(SlugRegex())
                .WithMessage("Tag slug must be lowercase and contain only letters, numbers, and hyphens.")
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
        }

        return builder;
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
