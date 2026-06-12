using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Validator for the <see cref="AdminAssignPermissionToRoleCommand" /> ensuring valid IDs.
/// </summary>
public class AdminAssignPermissionToRoleValidator : AbstractValidator<AdminAssignPermissionToRoleCommand>
{
    /// <summary>
    /// Configure validation rules for assigning a permission to a role.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminAssignPermissionToRoleValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.RoleId).IsValidGuid(i18n.Localizer, "RoleIdRequired", "RoleIdInvalid");
    }
}
