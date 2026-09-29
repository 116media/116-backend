using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;

/// <summary>
/// Validator for the <see cref="AdminCreateRoleCommand" /> ensuring proper role data format.
/// </summary>
public class AdminCreateRoleValidator : AbstractValidator<AdminCreateRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateRoleValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminCreateRoleValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Name).ValidRoleName(i18n.User.Validation);
        RuleFor(x => x.Description).ValidRoleDescription(i18n.User.Validation);
    }
}
