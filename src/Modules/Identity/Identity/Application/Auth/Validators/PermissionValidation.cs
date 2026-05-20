using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// <param name="msg">The validation error message provider.</param>
    /// <param name="isRequired">Whether the permission resource is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPermissionResource<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ValidationErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.PermissionResourceRequired())
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionResourceLength)
                .WithMessage(msg.PermissionResourceTooLong(PermissionConstants.MaxPermissionResourceLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: PermissionConstants.MaxPermissionResourceLength)
            .WithMessage(msg.PermissionResourceTooLong(PermissionConstants.MaxPermissionResourceLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Resource")));
    }

    /// <summary>
    /// Validates permission action with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the permission action property.</param>
    /// <param name="msg">The validation error message provider.</param>
    /// <param name="isRequired">Whether the permission action is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPermissionAction<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ValidationErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.PermissionActionRequired())
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionActionLength)
                .WithMessage(msg.PermissionActionTooLong(PermissionConstants.MaxPermissionActionLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: PermissionConstants.MaxPermissionActionLength)
            .WithMessage(msg.PermissionActionTooLong(PermissionConstants.MaxPermissionActionLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Action")));
    }

    /// <summary>
    /// Validates permission description with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the permission description property.</param>
    /// <param name="msg">The validation error message provider.</param>
    /// <param name="isRequired">Whether the permission description is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPermissionDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ValidationErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.PermissionDescriptionRequired())
                .MaximumLength(maximumLength: PermissionConstants.MaxPermissionDescriptionLength)
                .WithMessage(msg.PermissionDescriptionTooLong(PermissionConstants.MaxPermissionDescriptionLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: PermissionConstants.MaxPermissionDescriptionLength)
            .WithMessage(msg.PermissionDescriptionTooLong(PermissionConstants.MaxPermissionDescriptionLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Description")));
    }
}
