using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;

/// <summary>
/// Validator for the <see cref="AdminUpdatePermissionCommand" /> ensuring proper permission data format.
/// </summary>
public class AdminUpdatePermissionValidator : AbstractValidator<AdminUpdatePermissionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdatePermissionValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminUpdatePermissionValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.PermissionId).IsValidGuid("Permission ID");
        RuleFor(x => x.Action)
            .ValidPermissionAction(
                permissionActionRequired: msg.PermissionActionRequired(),
                permissionActionTooLong: msg.PermissionActionTooLong(PermissionConstants.MaxPermissionActionLength),
                isRequired: false
            );
        RuleFor(x => x.Resource)
            .ValidPermissionResource(
                permissionResourceRequired: msg.PermissionResourceRequired(),
                permissionResourceTooLong: msg.PermissionResourceTooLong(
                    PermissionConstants.MaxPermissionResourceLength
                ),
                isRequired: false
            );
        RuleFor(x => x.Description)
            .ValidPermissionDescription(
                permissionDescriptionRequired: msg.PermissionDescriptionRequired(),
                permissionDescriptionTooLong: msg.PermissionDescriptionTooLong(
                    PermissionConstants.MaxPermissionDescriptionLength
                ),
                isRequired: false
            );
    }
}
