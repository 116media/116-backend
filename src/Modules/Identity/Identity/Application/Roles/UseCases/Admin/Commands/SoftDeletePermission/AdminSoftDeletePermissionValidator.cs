using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Validator for the <see cref="AdminSoftDeletePermissionCommand" /> ensuring a valid permission ID.
/// </summary>
public class AdminSoftDeletePermissionValidator : AbstractValidator<AdminSoftDeletePermissionCommand>
{
    /// <summary>
    /// Configure validation rules for permission soft deletion.
    /// </summary>
    public AdminSoftDeletePermissionValidator()
    {
        RuleFor(x => x.PermissionId).IsValidGuid("Permission ID");
    }
}
