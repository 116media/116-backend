using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminSignOutValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken(refreshTokenRequired: msg.RefreshTokenRequired());
    }
}
