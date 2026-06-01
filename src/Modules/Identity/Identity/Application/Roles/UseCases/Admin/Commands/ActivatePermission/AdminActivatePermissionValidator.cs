using _116.Identity.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission;

/// <summary>
/// Validator for the <see cref="AdminActivatePermissionCommand" /> ensuring a valid permission ID.
/// </summary>
public class AdminActivatePermissionValidator : AbstractValidator<AdminActivatePermissionCommand>
{
    /// <summary>
    /// Configure validation rules for permission activation.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminActivatePermissionValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.PermissionId)
            .IsValidGuid(i18n.User.Validation.Localizer, "PermissionIdRequired", "PermissionIdInvalid");
    }
}
