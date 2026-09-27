using _116.BuildingBlocks.Presentation.Extensions;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession;

/// <summary>
/// Validator for the <see cref="AdminRevokeSessionCommand" /> ensuring valid GUIDs.
/// </summary>
public class AdminRevokeSessionValidator : AbstractValidator<AdminRevokeSessionCommand>
{
    /// <summary>
    /// Configure validation rules for revoke session requests.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminRevokeSessionValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.SessionId).IsValidGuid(i18n.User.Validation.Localizer, "SessionIdRequired", "SessionIdInvalid");
    }
}
