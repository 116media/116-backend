using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission;

/// <summary>
/// Validator for the <see cref="AdminDeactivatePermissionCommand" /> ensuring a valid permission ID.
/// </summary>
public class AdminDeactivatePermissionValidator : AbstractValidator<AdminDeactivatePermissionCommand>
{
    /// <summary>
    /// Configure validation rules for permission deactivation.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminDeactivatePermissionValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.PermissionId).IsValidGuid(i18n.Localizer, "PermissionIdRequired", "PermissionIdInvalid");
    }
}
