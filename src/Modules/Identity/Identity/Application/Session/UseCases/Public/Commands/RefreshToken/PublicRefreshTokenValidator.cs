using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;

/// <summary>
/// Validator for the <see cref="PublicRefreshTokenCommand" /> ensuring valid refresh token.
/// </summary>
public class PublicRefreshTokenValidator : AbstractValidator<PublicRefreshTokenCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicRefreshTokenValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public PublicRefreshTokenValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken(i18n.User.Validation);
    }
}
