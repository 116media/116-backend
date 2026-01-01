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
    public PublicRevokeSessionValidator()
    {
        RuleFor(x => x.SessionId).Cascade(CascadeMode.Stop).IsValidGuid("Session ID");
    }
}
