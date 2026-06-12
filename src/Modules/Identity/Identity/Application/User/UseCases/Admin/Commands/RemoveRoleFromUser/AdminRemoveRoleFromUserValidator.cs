using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Validator for the <see cref="AdminRemoveRoleFromUserCommand" /> ensuring a valid user ID.
/// </summary>
public class AdminRemoveRoleFromUserValidator : AbstractValidator<AdminRemoveRoleFromUserCommand>
{
    /// <summary>
    /// Configure validation rules for removing a role from a user.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminRemoveRoleFromUserValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.UserId).IsValidGuid(i18n.Localizer, "UserIdRequired", "UserIdInvalid");
        RuleFor(x => x.RoleId).IsValidGuid(i18n.Localizer, "RoleIdRequired", "RoleIdInvalid");
    }
}
