using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole;

/// <summary>
/// Validator for the <see cref="AdminActivateRoleCommand" /> ensuring a valid role ID.
/// </summary>
public class AdminActivateRoleValidator : AbstractValidator<AdminActivateRoleCommand>
{
    /// <summary>
    /// Configure validation rules for role activation.
    /// </summary>
    public AdminActivateRoleValidator()
    {
        RuleFor(x => x.RoleId).IsValidGuid("Role ID");
    }
}
