using FluentValidation;

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.SignOut;

/// <summary>
/// Validator for the <see cref="PublicSignOutCommand"/> ensuring proper refresh token format.
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
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}
