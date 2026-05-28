using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole;

/// <summary>
/// Validator for the <see cref="AdminRestoreRoleCommand" /> ensuring a valid role ID.
/// </summary>
public class AdminRestoreRoleValidator : AbstractValidator<AdminRestoreRoleCommand>
{
    /// <summary>
    /// Configure validation rules for role restoration.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminRestoreRoleValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.RoleId).IsValidGuid(i18n.Localizer, "RoleIdRequired", "RoleIdInvalid");
    }
}
