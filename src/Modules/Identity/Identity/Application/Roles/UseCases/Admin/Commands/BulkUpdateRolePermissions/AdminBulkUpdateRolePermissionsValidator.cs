using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions;

/// <summary>
/// Validator for the <see cref="AdminBulkUpdateRolePermissionsCommand" /> ensuring a valid role ID.
/// </summary>
public class AdminBulkUpdateRolePermissionsValidator : AbstractValidator<AdminBulkUpdateRolePermissionsCommand>
{
    /// <summary>
    /// Configure validation rules for bulk updating role permissions.
    /// </summary>
    public AdminBulkUpdateRolePermissionsValidator()
    {
        RuleFor(x => x.RoleId).IsValidGuid("Role ID");
    }
}
