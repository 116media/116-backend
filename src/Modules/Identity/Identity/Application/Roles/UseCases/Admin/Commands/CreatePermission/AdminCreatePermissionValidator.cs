using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;

/// <summary>
/// Validator for the <see cref="AdminCreatePermissionCommand" /> ensuring proper permission data format.
/// </summary>
public class AdminCreatePermissionValidator : AbstractValidator<AdminCreatePermissionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreatePermissionValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminCreatePermissionValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Resource).ValidPermissionResource(i18n.User.Validation);
        RuleFor(x => x.Action).ValidPermissionAction(i18n.User.Validation);
        RuleFor(x => x.Description).ValidPermissionDescription(i18n.User.Validation);
    }
}
