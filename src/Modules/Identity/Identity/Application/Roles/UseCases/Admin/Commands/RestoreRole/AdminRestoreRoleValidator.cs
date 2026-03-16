using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole;

/// <summary>
/// Validator for the <see cref="AdminRestoreRoleCommand" /> ensuring a valid role ID.
/// </summary>
public class AdminRestoreRoleValidator : AbstractValidator<AdminRestoreRoleCommand>
{
    /// <summary>
    /// Configure validation rules for role restoration.
    /// </summary>
    public AdminRestoreRoleValidator()
    {
        RuleFor(x => x.RoleId).IsValidGuid("Role ID");
    }
}
