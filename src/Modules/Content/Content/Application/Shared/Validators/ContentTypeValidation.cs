using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="msg">The error message provider for content type validation messages.</param>
    /// <param name="isRequired">Whether the name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidContentTypeName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ContentTypeErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.NameRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxContentTypeNameLength)
                .WithMessage(msg.NameTooLong(ContentConstants.MaxContentTypeNameLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxContentTypeNameLength)
            .WithMessage(msg.NameTooLong(ContentConstants.MaxContentTypeNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }
}
