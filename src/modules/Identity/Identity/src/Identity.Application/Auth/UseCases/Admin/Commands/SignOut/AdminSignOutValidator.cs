using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut;

/// <summary>
/// Validator for the <see cref="AdminSignOutCommand" /> ensuring proper refresh token format.
/// </summary>
/// <remarks>
/// Validates refresh token presence for session invalidation.
/// </remarks>
public class AdminSignOutValidator : AbstractValidator<AdminSignOutCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminSignOutValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminSignOutValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken(i18n.User.Validation);
    }
}
