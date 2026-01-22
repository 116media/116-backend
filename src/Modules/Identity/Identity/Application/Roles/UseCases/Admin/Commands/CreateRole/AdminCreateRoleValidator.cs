using _116.Identity.Application.Auth.Validators;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;

/// <summary>
/// Validator for the <see cref="AdminCreateRoleCommand" /> ensuring proper role data format.
/// </summary>
public class AdminCreateRoleValidator : AbstractValidator<AdminCreateRoleCommand>
{
    /// <summary>
    /// Configure validation rules for role creation.
    /// </summary>
    public AdminCreateRoleValidator()
    {
        RuleFor(x => x.Name).ValidRoleName();
        RuleFor(x => x.Description).ValidRoleDescription();
    }
}
