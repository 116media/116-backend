using _116.BuildingBlocks.Constants;
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
        RuleFor(x => x.Resource)
            .ValidPermissionResource(
                permissionResourceRequired: msg.PermissionResourceRequired(),
                permissionResourceTooLong: msg.PermissionResourceTooLong(
                    PermissionConstants.MaxPermissionResourceLength
                )
            );
        RuleFor(x => x.Action)
            .ValidPermissionAction(
                permissionActionRequired: msg.PermissionActionRequired(),
                permissionActionTooLong: msg.PermissionActionTooLong(PermissionConstants.MaxPermissionActionLength)
            );
        RuleFor(x => x.Description)
            .ValidPermissionDescription(
                permissionDescriptionRequired: msg.PermissionDescriptionRequired(),
                permissionDescriptionTooLong: msg.PermissionDescriptionTooLong(
                    PermissionConstants.MaxPermissionDescriptionLength
                )
            );
    }
}
