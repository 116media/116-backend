using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
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
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminHardDeleteRoleValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.RoleId).IsValidGuid(i18n.Localizer, "RoleIdRequired", "RoleIdInvalid");
    }
}
