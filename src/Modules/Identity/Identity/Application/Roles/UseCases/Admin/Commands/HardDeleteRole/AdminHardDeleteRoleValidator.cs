using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole;

/// <summary>
/// Validator for the <see cref="AdminHardDeleteRoleCommand" /> ensuring a valid role ID.
/// </summary>
public class AdminHardDeleteRoleValidator : AbstractValidator<AdminHardDeleteRoleCommand>
{
    /// <summary>
    /// Configure validation rules for role hard deletion.
    /// </summary>
    public AdminHardDeleteRoleValidator()
    {
        RuleFor(x => x.RoleId).IsValidGuid("Role ID");
    }
}
