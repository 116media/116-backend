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
    public AdminForceLogoutUserValidator()
    {
        RuleFor(x => x.UserId).Cascade(CascadeMode.Stop).IsValidGuid("User ID");
    }
}
