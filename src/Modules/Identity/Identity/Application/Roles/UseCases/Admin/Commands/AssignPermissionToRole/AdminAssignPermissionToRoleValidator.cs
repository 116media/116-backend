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
    public AdminAssignPermissionToRoleValidator()
    {
        RuleFor(x => x.RoleId).IsValidGuid("Role ID");
    }
}
