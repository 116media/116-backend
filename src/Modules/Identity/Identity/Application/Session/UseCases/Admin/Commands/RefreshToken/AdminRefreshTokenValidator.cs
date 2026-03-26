using _116.Identity.Application.Auth.Validators;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.RefreshToken;

/// <summary>
/// Validator for the <see cref="AdminRefreshTokenCommand" /> ensuring valid refresh token.
/// </summary>
public class AdminRefreshTokenValidator : AbstractValidator<AdminRefreshTokenCommand>
{
    /// <summary>
    /// Configure validation rules for admin refresh token requests.
    /// </summary>
    public AdminRefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken();
    }
}
