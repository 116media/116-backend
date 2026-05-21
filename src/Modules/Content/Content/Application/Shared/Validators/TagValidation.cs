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
    /// <param name="nameRequired">Error message used when the name is empty.</param>
    /// <param name="nameTooLong">Error message used when the name exceeds the maximum length.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidTagName<T>(
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
                .MaximumLength(maximumLength: ContentConstants.MaxTagNameLength)
                .WithMessage(nameTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTagNameLength)
            .WithMessage(nameTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates tag slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="slugRequired">Error message used when the slug is empty.</param>
    /// <param name="slugTooLong">Error message used when the slug exceeds the maximum length.</param>
    /// <param name="slugInvalidFormat">Error message used when the slug does not match the expected format.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidTagSlug<T>(
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
                .MaximumLength(maximumLength: ContentConstants.MaxTagSlugLength)
                .WithMessage(slugTooLong)
                .Matches(SlugRegex())
                .WithMessage(slugInvalidFormat);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTagSlugLength)
            .WithMessage(slugTooLong)
            .Matches(SlugRegex())
            .WithMessage(slugInvalidFormat)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates each element of a tag name collection: not empty and within the max length.
    /// Intended for use with <c>RuleForEach</c>.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for each element in the tag names collection.</param>
    /// <param name="nameRequired">Error message used when a tag name element is empty.</param>
    /// <param name="nameTooLong">Error message used when a tag name element exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidTagNameItem<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        string nameRequired,
        string nameTooLong
    )
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(nameRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxTagNameLength)
            .WithMessage(nameTooLong);
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
