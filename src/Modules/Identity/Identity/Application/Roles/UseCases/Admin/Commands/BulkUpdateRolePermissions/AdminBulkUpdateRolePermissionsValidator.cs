using _116.Identity.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminBulkUpdateRolePermissionsValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.RoleId).IsValidGuid(i18n.User.Validation.Localizer, "RoleIdRequired", "RoleIdInvalid");
    }
}
