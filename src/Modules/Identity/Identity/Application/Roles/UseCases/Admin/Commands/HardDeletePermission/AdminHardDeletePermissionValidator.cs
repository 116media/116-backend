using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission;

/// <summary>
/// Validator for the <see cref="AdminHardDeletePermissionCommand" /> ensuring a valid permission ID.
/// </summary>
public class AdminHardDeletePermissionValidator : AbstractValidator<AdminHardDeletePermissionCommand>
{
    /// <summary>
    /// Configure validation rules for permission hard deletion.
    /// </summary>
    public AdminHardDeletePermissionValidator()
    {
        RuleFor(x => x.PermissionId).IsValidGuid("Permission ID");
    }
}
