using _116.BuildingBlocks.Constants;
using FluentValidation;

namespace _116.Identity.Application.Auth.Validators;

/// <summary>
/// FluentValidation extensions for permission field validation (resource, action, description).
/// </summary>
public static class PermissionValidation
{
    /// <summary>
    /// Validates permission resource with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the permission resource property.</param>
    /// <param name="isRequired">Whether the permission resource is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPermissionResource<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty()
                .WithMessage("Permission resource is required")
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionResourceLength)
                .WithMessage(
                    $"Permission resource cannot exceed {PermissionConstants.MaxPermissionResourceLength} characters"
                );
        }
        else
        {
            builder = ruleBuilder
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionResourceLength)
                .WithMessage(
                    $"Permission resource cannot exceed {PermissionConstants.MaxPermissionResourceLength} characters"
                )
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Resource")));
        }

        return builder;
    }

    /// <summary>
    /// Validates permission action with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the permission action property.</param>
    /// <param name="isRequired">Whether the permission action is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPermissionAction<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty()
                .WithMessage("Permission action is required")
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionActionLength)
                .WithMessage(
                    $"Permission action cannot exceed {PermissionConstants.MaxPermissionActionLength} characters"
                );
        }
        else
        {
            builder = ruleBuilder
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionActionLength)
                .WithMessage(
                    $"Permission action cannot exceed {PermissionConstants.MaxPermissionActionLength} characters"
                )
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Action")));
        }

        return builder;
    }

    /// <summary>
    /// Validates permission description with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the permission description property.</param>
    /// <param name="isRequired">Whether the permission description is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPermissionDescription<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        IRuleBuilderOptions<T, string?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotEmpty()
                .WithMessage("Permission description is required")
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionDescriptionLength)
                .WithMessage(
                    $"Permission description cannot exceed {PermissionConstants.MaxPermissionDescriptionLength} characters"
                );
        }
        else
        {
            builder = ruleBuilder
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionDescriptionLength)
                .WithMessage(
                    $"Permission description cannot exceed {PermissionConstants.MaxPermissionDescriptionLength} characters"
                )
                .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Description")));
        }

        return builder;
    }
}
