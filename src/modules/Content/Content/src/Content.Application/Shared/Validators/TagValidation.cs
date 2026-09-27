using System.Text.RegularExpressions;
using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">The tag error message provider.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidTagName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        TagErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.NameRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxTagNameLength)
                .WithMessage(i18n.NameTooLong(ContentConstants.MaxTagNameLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTagNameLength)
            .WithMessage(i18n.NameTooLong(ContentConstants.MaxTagNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates tag slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="i18n">The tag error message provider.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidTagSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        TagErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.SlugRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxTagSlugLength)
                .WithMessage(i18n.SlugTooLong(ContentConstants.MaxTagSlugLength))
                .Matches(SlugRegex())
                .WithMessage(i18n.SlugInvalidFormat());
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTagSlugLength)
            .WithMessage(i18n.SlugTooLong(ContentConstants.MaxTagSlugLength))
            .Matches(SlugRegex())
            .WithMessage(i18n.SlugInvalidFormat())
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates each element of a tag name collection: not empty and within the max length.
    /// Intended for use with <c>RuleForEach</c>.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for each element in the tag names collection.</param>
    /// <param name="i18n">The tag error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidTagNameItem<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        TagErrorMessage i18n
    )
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(i18n.NameRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxTagNameLength)
            .WithMessage(i18n.NameTooLong(ContentConstants.MaxTagNameLength));
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
