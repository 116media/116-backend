using _116.Identity.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.DeactivateUser;

/// <summary>
/// Validator for the <see cref="AdminDeactivateUserCommand" /> ensuring a valid user ID.
/// </summary>
public class AdminDeactivateUserValidator : AbstractValidator<AdminDeactivateUserCommand>
{
    /// <summary>
    /// Configure validation rules for deactivate-user requests.
    /// </summary>
    /// <param name="i18n">Identity module i18n facade for rule configuration.</param>
    public AdminDeactivateUserValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.UserId).IsValidGuid(i18n.User.Validation.Localizer, "UserIdRequired", "UserIdInvalid");
    }
}
