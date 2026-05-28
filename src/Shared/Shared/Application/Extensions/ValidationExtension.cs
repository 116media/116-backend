using FluentValidation;
using Microsoft.Extensions.Localization;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Provides custom FluentValidation extensions for commonly used validation rules.
/// </summary>
public static class ValidationExtension
{
    /// <summary>
    /// Adds a validation rule to ensure that a string property is a valid GUID.
    /// Reads localized "IdRequired" and "IdInvalid" messages from the provided localizer.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object being validated.
    /// </typeparam>
    /// <param name="ruleBuilder">
    /// The FluentValidation rule builder for the property.
    /// </param>
    /// <param name="localizer">
    /// The string localizer containing "IdRequired" and "IdInvalid" keys.
    /// Typically obtained via <c>errorMessage.Localizer</c>.
    /// </param>
    /// <param name="isRequired">
    /// Whether the GUID is required (defaults to true).
    /// If false, only validates the format when provided.
    /// </param>
    /// <example>
    /// <code>
    /// // Required GUID validation with localized messages
    /// RuleFor(x => x.UserId).IsValidGuid(i18n.Localizer);
    ///
    /// // Optional GUID validation (only validates if provided)
    /// RuleFor(x => x.UserId).IsValidGuid(i18n.Localizer, isRequired: false);
    /// </code>
    /// </example>
    public static IRuleBuilderOptions<T, string?> IsValidGuid<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        IStringLocalizer localizer,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(localizer["IdRequired"].Value)
                .Must(id => Guid.TryParse(input: id, result: out _))
                .WithMessage(localizer["IdInvalid"].Value);
        }

        return ruleBuilder
            .Must(id => string.IsNullOrWhiteSpace(value: id) || Guid.TryParse(input: id, result: out _))
            .WithMessage(localizer["IdInvalid"].Value);
    }

    /// <summary>
    /// Adds a validation rule to ensure that a string property is a valid GUID,
    /// using domain-specific localization keys for the required and invalid messages.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object being validated.
    /// </typeparam>
    /// <param name="ruleBuilder">
    /// The FluentValidation rule builder for the property.
    /// </param>
    /// <param name="localizer">
    /// The string localizer used to resolve error messages.
    /// </param>
    /// <param name="requiredKey">
    /// The localization key for the "required" error message (e.g., "RoleIdRequired").
    /// </param>
    /// <param name="invalidKey">
    /// The localization key for the "invalid" error message (e.g., "RoleIdInvalid").
    /// </param>
    /// <param name="isRequired">
    /// Whether the GUID is required (defaults to true).
    /// </param>
    public static IRuleBuilderOptions<T, string?> IsValidGuid<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        IStringLocalizer localizer,
        string requiredKey,
        string invalidKey,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(localizer[requiredKey].Value)
                .Must(id => Guid.TryParse(input: id, result: out _))
                .WithMessage(localizer[invalidKey].Value);
        }

        return ruleBuilder
            .Must(id => string.IsNullOrWhiteSpace(value: id) || Guid.TryParse(input: id, result: out _))
            .WithMessage(localizer[invalidKey].Value);
    }
}
