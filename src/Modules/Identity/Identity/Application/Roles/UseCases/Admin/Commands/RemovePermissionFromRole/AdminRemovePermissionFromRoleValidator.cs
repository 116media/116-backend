using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Validator for the <see cref="AdminRemovePermissionFromRoleCommand" /> ensuring valid IDs.
/// </summary>
public class AdminRemovePermissionFromRoleValidator : AbstractValidator<AdminRemovePermissionFromRoleCommand>
{
    /// <summary>
    /// Configure validation rules for removing a permission from a role.
    /// </summary>
    public AdminRemovePermissionFromRoleValidator()
    {
        RuleFor(x => x.RoleId).IsValidGuid("Role ID");
    }
}
