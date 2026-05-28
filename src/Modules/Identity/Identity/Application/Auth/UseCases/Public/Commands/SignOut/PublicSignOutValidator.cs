using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;

/// <summary>
/// Validator for the <see cref="PublicSignOutCommand" /> ensuring proper refresh token format.
/// </summary>
/// <remarks>
/// Validates refresh token presence for session invalidation.
/// </remarks>
public class PublicSignOutValidator : AbstractValidator<PublicSignOutCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicSignOutValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicSignOutValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken(i18n);
    }
}
