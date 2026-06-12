using _116.Identity.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RevokeSession;

/// <summary>
/// Validator for the <see cref="PublicRevokeSessionCommand" /> ensuring valid GUIDs.
/// </summary>
public class PublicRevokeSessionValidator : AbstractValidator<PublicRevokeSessionCommand>
{
    /// <summary>
    /// Configure validation rules for revoke session requests.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public PublicRevokeSessionValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.SessionId).IsValidGuid(i18n.User.Validation.Localizer, "SessionIdRequired", "SessionIdInvalid");
    }
}
