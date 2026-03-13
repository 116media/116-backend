using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;

/// <summary>
/// Validator for the <see cref="AdminSoftDeleteRoleCommand" /> ensuring a valid role ID.
/// </summary>
public class AdminSoftDeleteRoleValidator : AbstractValidator<AdminSoftDeleteRoleCommand>
{
    /// <summary>
    /// Configure validation rules for role soft deletion.
    /// </summary>
    public AdminSoftDeleteRoleValidator()
    {
        RuleFor(x => x.RoleId).IsValidGuid("Role ID");
    }
}
