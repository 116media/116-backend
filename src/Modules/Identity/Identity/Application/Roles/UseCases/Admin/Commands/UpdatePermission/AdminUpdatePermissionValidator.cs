using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
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
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminUpdatePermissionValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.PermissionId).IsValidGuid(i18n.User.Validation.Localizer);
        RuleFor(x => x.Action).ValidPermissionAction(i18n.User.Validation, isRequired: false);
        RuleFor(x => x.Resource).ValidPermissionResource(i18n.User.Validation, isRequired: false);
        RuleFor(x => x.Description).ValidPermissionDescription(i18n.User.Validation, isRequired: false);
    }
}
