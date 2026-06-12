using _116.Identity.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminRemovePermissionFromRoleValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.RoleId).IsValidGuid(i18n.User.Validation.Localizer, "RoleIdRequired", "RoleIdInvalid");
        RuleFor(x => x.PermissionId)
            .IsValidGuid(i18n.User.Validation.Localizer, "PermissionIdRequired", "PermissionIdInvalid");
    }
}
