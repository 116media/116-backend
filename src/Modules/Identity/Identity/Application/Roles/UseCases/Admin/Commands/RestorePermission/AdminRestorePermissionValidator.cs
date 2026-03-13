using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission;

/// <summary>
/// Validator for the <see cref="AdminRestorePermissionCommand" /> ensuring a valid permission ID.
/// </summary>
public class AdminRestorePermissionValidator : AbstractValidator<AdminRestorePermissionCommand>
{
    /// <summary>
    /// Configure validation rules for permission restoration.
    /// </summary>
    public AdminRestorePermissionValidator()
    {
        RuleFor(x => x.PermissionId).IsValidGuid("Permission ID");
    }
}
