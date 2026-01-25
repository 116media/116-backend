using _116.Identity.Application.Auth.Validators;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;

/// <summary>
/// Validator for the <see cref="AdminCreatePermissionCommand" /> ensuring proper permission data format.
/// </summary>
public class AdminCreatePermissionValidator : AbstractValidator<AdminCreatePermissionCommand>
{
    /// <summary>
    /// Configure validation rules for permission creation.
    /// </summary>
    public AdminCreatePermissionValidator()
    {
        RuleFor(x => x.Resource).ValidPermissionResource();
        RuleFor(x => x.Action).ValidPermissionAction();
        RuleFor(x => x.Description).ValidPermissionDescription();
    }
}
