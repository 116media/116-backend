using _116.Identity.Application.Shared.Errors.Facade;
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
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminRemoveRoleFromUserValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.UserId).IsValidGuid(i18n.User.Validation.Localizer, "UserIdRequired", "UserIdInvalid");
        RuleFor(x => x.RoleId).IsValidGuid(i18n.User.Validation.Localizer, "RoleIdRequired", "RoleIdInvalid");
    }
}
