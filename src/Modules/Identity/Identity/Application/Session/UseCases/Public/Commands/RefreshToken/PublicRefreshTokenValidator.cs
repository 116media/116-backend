using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;

/// <summary>
/// Validator for the <see cref="PublicRefreshTokenCommand" /> ensuring valid refresh token.
/// </summary>
public class PublicRefreshTokenValidator : AbstractValidator<PublicRefreshTokenCommand>
{
    /// <summary>
    /// Configure validation rules for refresh token requests.
    /// </summary>
    public PublicRefreshTokenValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken(msg);
    }
}
