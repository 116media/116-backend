using _116.Identity.Application.Auth.Validators;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Validator for the <see cref="AdminLoginCommand" /> ensuring proper admin credential format.
/// </summary>
public class AdminLoginValidator : AbstractValidator<AdminLoginCommand>
{
    /// <summary>
    /// Configure validation rules for admin authentication.
    /// </summary>
    /// <remarks>
    /// Validates email and password presence for admin login attempts.
    /// </remarks>
    public AdminLoginValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).ValidPassword(isStrong: false);
    }
}
