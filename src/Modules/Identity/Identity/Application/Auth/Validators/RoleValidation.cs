using _116.BuildingBlocks.Constants;
using FluentValidation;

namespace _116.Identity.Application.Auth.Validators;

/// <summary>
/// FluentValidation extensions for role field validation (name, description).
/// </summary>
public static class RoleValidation
{
    /// <summary>
    /// Validates role name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the role name property.</param>
    /// <param name="roleNameRequired">Error message when role name is missing.</param>
    /// <param name="roleNameTooLong">Error message when role name exceeds maximum length.</param>
    /// <param name="isRequired">Whether the role name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidRoleName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string roleNameRequired,
        string roleNameTooLong,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(roleNameRequired)
                .MaximumLength(maximumLength: RoleConstants.MaxRoleNameLength)
                .WithMessage(roleNameTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: RoleConstants.MaxRoleNameLength)
            .WithMessage(roleNameTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates role description with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the role description property.</param>
    /// <param name="roleDescriptionRequired">Error message when role description is missing.</param>
    /// <param name="roleDescriptionTooLong">Error message when role description exceeds maximum length.</param>
    /// <param name="isRequired">Whether the role description is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidRoleDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string roleDescriptionRequired,
        string roleDescriptionTooLong,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(roleDescriptionRequired)
                .MaximumLength(maximumLength: RoleConstants.MaxRoleDescriptionLength)
                .WithMessage(roleDescriptionTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: RoleConstants.MaxRoleDescriptionLength)
            .WithMessage(roleDescriptionTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Description")));
    }
}
