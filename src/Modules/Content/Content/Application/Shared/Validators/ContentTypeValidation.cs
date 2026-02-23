using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for content type field validation (id, name).
/// </summary>
public static class ContentTypeValidation
{
    /// <summary>
    /// Validates that a content type ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the ID property.</param>
    /// <returns>The configured rule builder.</returns>
    public static void ValidContentTypeId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        ruleBuilder.NotEmpty().WithMessage("Content type ID is required.");
    }

    /// <summary>
    /// Validates content type name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidContentTypeName<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty()
                .WithMessage("Content type name is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxContentTypeNameLength)
                .WithMessage(
                    $"Content type name must not exceed {ContentConstants.MaxContentTypeNameLength} characters."
                );
        }
        else
        {
            builder = ruleBuilder
                .MaximumLength(maximumLength: ContentConstants.MaxContentTypeNameLength)
                .WithMessage(
                    $"Content type name must not exceed {ContentConstants.MaxContentTypeNameLength} characters."
                )
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
        }

        return builder;
    }
}
