using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;

/// <summary>
/// Validator for the <see cref="AdminUpdatePermissionCommand" /> ensuring proper permission data format.
/// </summary>
public class AdminUpdatePermissionValidator : AbstractValidator<AdminUpdatePermissionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdatePermissionValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminUpdatePermissionValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.PermissionId).IsValidGuid(i18n.Localizer);
        RuleFor(x => x.Action).ValidPermissionAction(i18n, isRequired: false);
        RuleFor(x => x.Resource).ValidPermissionResource(i18n, isRequired: false);
        RuleFor(x => x.Description).ValidPermissionDescription(i18n, isRequired: false);
    }
}
