using _116.Identity.Application.Auth.Validators;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;

/// <summary>
/// Validator for the <see cref="AdminUpdatePermissionCommand" /> ensuring proper permission data format.
/// </summary>
public class AdminUpdatePermissionValidator : AbstractValidator<AdminUpdatePermissionCommand>
{
    /// <summary>
    /// Configure validation rules for permission update.
    /// </summary>
    public AdminUpdatePermissionValidator()
    {
        RuleFor(x => x.Action).ValidPermissionAction(false);
        RuleFor(x => x.Resource).ValidPermissionResource(false);
        RuleFor(x => x.Description).ValidPermissionDescription(false);
    }
}
