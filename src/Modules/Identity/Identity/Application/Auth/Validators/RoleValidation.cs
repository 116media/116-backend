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
    /// <param name="isRequired">Whether the role name is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidRoleName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Role name is required")
                .MaximumLength(maximumLength: RoleConstants.MaxRoleNameLength)
                .WithMessage($"Role name cannot exceed {RoleConstants.MaxRoleNameLength} characters");
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: RoleConstants.MaxRoleNameLength)
            .WithMessage($"Role name cannot exceed {RoleConstants.MaxRoleNameLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Name")));
    }

    /// <summary>
    /// Validates role description with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the role description property.</param>
    /// <param name="isRequired">Whether the role description is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidRoleDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Role description is required")
                .MaximumLength(maximumLength: RoleConstants.MaxRoleDescriptionLength)
                .WithMessage($"Role description cannot exceed {RoleConstants.MaxRoleDescriptionLength} characters");
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: RoleConstants.MaxRoleDescriptionLength)
            .WithMessage($"Role description cannot exceed {RoleConstants.MaxRoleDescriptionLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Description")));
    }
}
