using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for content type field validation (id, name).
/// </summary>
public static class ContentTypeValidation
{
    /// <summary>
    /// Validates content type name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidContentTypeName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Content type name is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxContentTypeNameLength)
                .WithMessage(
                    $"Content type name must not exceed {ContentConstants.MaxContentTypeNameLength} characters."
                );
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxContentTypeNameLength)
            .WithMessage($"Content type name must not exceed {ContentConstants.MaxContentTypeNameLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }
}
