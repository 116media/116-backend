using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Validator for the <see cref="AdminAssignRoleToUserCommand" /> ensuring a valid user ID.
/// </summary>
public class AdminAssignRoleToUserValidator : AbstractValidator<AdminAssignRoleToUserCommand>
{
    /// <summary>
    /// Configure validation rules for assigning a role to a user.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminAssignRoleToUserValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.UserId).IsValidGuid(i18n.Localizer, "UserIdRequired", "UserIdInvalid");
    }
}
