using _116.Identity.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;

/// <summary>
/// Validator for the <see cref="AdminForceLogoutUserCommand" /> ensuring valid GUID.
/// </summary>
public class AdminForceLogoutUserValidator : AbstractValidator<AdminForceLogoutUserCommand>
{
    /// <summary>
    /// Configure validation rules for force logout requests.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminForceLogoutUserValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.UserId).IsValidGuid(i18n.User.Validation.Localizer, "UserIdRequired", "UserIdInvalid");
    }
}
