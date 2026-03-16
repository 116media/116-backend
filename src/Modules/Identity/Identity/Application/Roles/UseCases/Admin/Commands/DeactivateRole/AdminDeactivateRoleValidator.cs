using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole;

/// <summary>
/// Validator for the <see cref="AdminDeactivateRoleCommand" /> ensuring a valid role ID.
/// </summary>
public class AdminDeactivateRoleValidator : AbstractValidator<AdminDeactivateRoleCommand>
{
    /// <summary>
    /// Configure validation rules for role deactivation.
    /// </summary>
    public AdminDeactivateRoleValidator()
    {
        RuleFor(x => x.RoleId).IsValidGuid("Role ID");
    }
}
