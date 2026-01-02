using FluentValidation;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Provides custom FluentValidation extensions for commonly used validation rules.
/// </summary>
public static class ValidationExtension
{
    /// <summary>
    /// Adds a validation rule to ensure that a string property is a valid GUID.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The FluentValidation rule builder for the property.</param>
    /// <param name="fieldDisplayName">Optional display name used in validation messages (defaults to "Id").</param>
    /// <param name="isRequired">Whether the GUID is required (defaults to true). If false, only validates format when provided.</param>
    /// <example>
    /// <code>
    /// // Required GUID validation
    /// RuleFor(x => x.UserId).IsValidGuid("User ID");
    ///
    /// // Optional GUID validation (only validates if provided)
    /// RuleFor(x => x.UserId).IsValidGuid("User ID", isRequired: false);
    /// </code>
    /// </example>
    public static IRuleBuilderOptions<T, string?> IsValidGuid<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string fieldDisplayName = "Id",
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty().WithMessage($"{fieldDisplayName} is required.")
                .Must(id => Guid.TryParse(input: id, result: out _))
                .WithMessage($"{fieldDisplayName} is invalid.");
        }
        else
        {
            builder = ruleBuilder
                .Must(id => string.IsNullOrWhiteSpace(value: id) || Guid.TryParse(input: id, result: out _))
                .WithMessage($"{fieldDisplayName} is invalid.");
        }

        return builder;
    }
}
