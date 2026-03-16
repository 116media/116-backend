using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission;

/// <summary>
/// Validator for the <see cref="AdminActivatePermissionCommand" /> ensuring a valid permission ID.
/// </summary>
public class AdminActivatePermissionValidator : AbstractValidator<AdminActivatePermissionCommand>
{
    /// <summary>
    /// Configure validation rules for permission activation.
    /// </summary>
    public AdminActivatePermissionValidator()
    {
        RuleFor(x => x.PermissionId).IsValidGuid("Permission ID");
    }
}
