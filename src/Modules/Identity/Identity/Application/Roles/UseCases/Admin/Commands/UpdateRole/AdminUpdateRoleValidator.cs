using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;

/// <summary>
/// Validator for the <see cref="AdminUpdateRoleCommand" /> ensuring proper role data format.
/// </summary>
public class AdminUpdateRoleValidator : AbstractValidator<AdminUpdateRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateRoleValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminUpdateRoleValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.RoleId).IsValidGuid(i18n.User.Validation.Localizer);
        RuleFor(x => x.Name).ValidRoleName(i18n.User.Validation, isRequired: false);
        RuleFor(x => x.Description).ValidRoleDescription(i18n.User.Validation, isRequired: false);
    }
}
