using _116.Identity.Application.Shared.Errors.Messages;
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
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminActivatePermissionValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.PermissionId).IsValidGuid(i18n.Localizer, "PermissionIdRequired", "PermissionIdInvalid");
    }
}
