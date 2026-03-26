using _116.Identity.Application.Auth.Validators;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;

/// <summary>
/// Validator for the <see cref="PublicSignOutCommand" /> ensuring proper refresh token format.
/// </summary>
public class PublicSignOutValidator : AbstractValidator<PublicSignOutCommand>
{
    /// <summary>
    /// Configure validation rules for public user sign-out.
    /// </summary>
    /// <remarks>
    /// Validates refresh token presence for session invalidation.
    /// </remarks>
    public PublicSignOutValidator()
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken();
    }
}
