using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.RefreshToken;

/// <summary>
/// Validator for the <see cref="AdminRefreshTokenCommand" /> ensuring valid refresh token.
/// </summary>
public class AdminRefreshTokenValidator : AbstractValidator<AdminRefreshTokenCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRefreshTokenValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminRefreshTokenValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken(i18n.User.Validation);
    }
}
