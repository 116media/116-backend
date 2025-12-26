using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut;

/// <summary>
/// Validator for the <see cref="AdminSignOutCommand" /> ensuring proper refresh token format.
/// </summary>
public class AdminSignOutValidator : AbstractValidator<AdminSignOutCommand>
{
    /// <summary>
    /// Configure validation rules for admin user sign-out.
    /// </summary>
    /// <remarks>
    /// Validates refresh token presence for session invalidation.
    /// </remarks>
    public AdminSignOutValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}
