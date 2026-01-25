using _116.Identity.Application.Auth.Validators;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;

/// <summary>
/// Validator for the <see cref="AdminUpdateRoleCommand" /> ensuring proper role data format.
/// </summary>
public class AdminUpdateRoleValidator : AbstractValidator<AdminUpdateRoleCommand>
{
    /// <summary>
    /// Configure validation rules for role update.
    /// </summary>
    public AdminUpdateRoleValidator()
    {
        RuleFor(x => x.Name).ValidRoleName(false);
        RuleFor(x => x.Description).ValidRoleDescription(false);
    }
}
