using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Validator for the <see cref="AdminSoftDeletePermissionCommand" /> ensuring a valid permission ID.
/// </summary>
public class AdminSoftDeletePermissionValidator : AbstractValidator<AdminSoftDeletePermissionCommand>
{
    /// <summary>
    /// Configure validation rules for permission soft deletion.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminSoftDeletePermissionValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.PermissionId).IsValidGuid(i18n.Localizer, "PermissionIdRequired", "PermissionIdInvalid");
    }
}
