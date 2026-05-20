using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;

/// <summary>
/// Validator for the <see cref="AdminCreatePermissionCommand" /> ensuring proper permission data format.
/// </summary>
public class AdminCreatePermissionValidator : AbstractValidator<AdminCreatePermissionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreatePermissionValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminCreatePermissionValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Resource).ValidPermissionResource(msg);
        RuleFor(x => x.Action).ValidPermissionAction(msg);
        RuleFor(x => x.Description).ValidPermissionDescription(msg);
    }
}
