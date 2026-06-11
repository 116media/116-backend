using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminRefreshTokenValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken(refreshTokenRequired: msg.RefreshTokenRequired());
    }
}
