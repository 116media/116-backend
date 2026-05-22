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
    /// <param name="permissionResourceRequired">Error message when permission resource is missing.</param>
    /// <param name="permissionResourceTooLong">Error message when permission resource exceeds maximum length.</param>
    /// <param name="isRequired">Whether the permission resource is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPermissionResource<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string permissionResourceRequired,
        string permissionResourceTooLong,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(permissionResourceRequired)
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionResourceLength)
                .WithMessage(permissionResourceTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: PermissionConstants.MaxPermissionResourceLength)
            .WithMessage(permissionResourceTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Resource")));
    }

    /// <summary>
    /// Validates permission action with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the permission action property.</param>
    /// <param name="permissionActionRequired">Error message when permission action is missing.</param>
    /// <param name="permissionActionTooLong">Error message when permission action exceeds maximum length.</param>
    /// <param name="isRequired">Whether the permission action is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPermissionAction<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string permissionActionRequired,
        string permissionActionTooLong,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(permissionActionRequired)
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionActionLength)
                .WithMessage(permissionActionTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: PermissionConstants.MaxPermissionActionLength)
            .WithMessage(permissionActionTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Action")));
    }

    /// <summary>
    /// Validates permission description with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the permission description property.</param>
    /// <param name="permissionDescriptionRequired">Error message when permission description is missing.</param>
    /// <param name="permissionDescriptionTooLong">Error message when permission description exceeds maximum length.</param>
    /// <param name="isRequired">Whether the permission description is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPermissionDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string permissionDescriptionRequired,
        string permissionDescriptionTooLong,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(permissionDescriptionRequired)
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionDescriptionLength)
                .WithMessage(permissionDescriptionTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: PermissionConstants.MaxPermissionDescriptionLength)
            .WithMessage(permissionDescriptionTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Description")));
    }
}
