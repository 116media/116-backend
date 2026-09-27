using _116.BuildingBlocks.Presentation.Extensions;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole;

/// <summary>
/// Validator for the <see cref="AdminHardDeleteRoleCommand" /> ensuring a valid role ID.
/// </summary>
public class AdminHardDeleteRoleValidator : AbstractValidator<AdminHardDeleteRoleCommand>
{
    /// <summary>
    /// Configure validation rules for role hard deletion.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminHardDeleteRoleValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.RoleId).IsValidGuid(i18n.User.Validation.Localizer, "RoleIdRequired", "RoleIdInvalid");
    }
}
