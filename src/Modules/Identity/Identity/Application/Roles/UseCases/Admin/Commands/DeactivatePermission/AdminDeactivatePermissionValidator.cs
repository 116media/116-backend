using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission;

/// <summary>
/// Validator for the <see cref="AdminDeactivatePermissionCommand" /> ensuring a valid permission ID.
/// </summary>
public class AdminDeactivatePermissionValidator : AbstractValidator<AdminDeactivatePermissionCommand>
{
    /// <summary>
    /// Configure validation rules for permission deactivation.
    /// </summary>
    public AdminDeactivatePermissionValidator()
    {
        RuleFor(x => x.PermissionId).IsValidGuid("Permission ID");
    }
}
