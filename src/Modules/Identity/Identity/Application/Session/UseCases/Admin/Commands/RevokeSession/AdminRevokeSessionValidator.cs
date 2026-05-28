using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
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
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminRevokeSessionValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.SessionId).IsValidGuid(i18n.Localizer, "SessionIdRequired", "SessionIdInvalid");
    }
}
