using _116.BuildingBlocks.Presentation.Extensions;
using _116.Identity.Application.Shared.Errors.Facade;
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
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminDeactivatePermissionValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.PermissionId)
            .IsValidGuid(i18n.User.Validation.Localizer, "PermissionIdRequired", "PermissionIdInvalid");
    }
}
