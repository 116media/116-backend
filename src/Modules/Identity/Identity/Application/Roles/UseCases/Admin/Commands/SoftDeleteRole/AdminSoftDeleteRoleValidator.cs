using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;

/// <summary>
/// Validator for the <see cref="AdminSoftDeleteRoleCommand" /> ensuring a valid role ID.
/// </summary>
public class AdminSoftDeleteRoleValidator : AbstractValidator<AdminSoftDeleteRoleCommand>
{
    /// <summary>
    /// Configure validation rules for role soft deletion.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminSoftDeleteRoleValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.RoleId).IsValidGuid(i18n.Localizer, "RoleIdRequired", "RoleIdInvalid");
    }
}
