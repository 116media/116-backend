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
    public AdminRevokeSessionValidator()
    {
        RuleFor(x => x.SessionId).IsValidGuid("Session ID");
    }
}
