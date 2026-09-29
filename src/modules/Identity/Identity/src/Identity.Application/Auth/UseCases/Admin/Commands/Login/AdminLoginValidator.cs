using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Validator for the <see cref="AdminLoginCommand" /> ensuring proper admin credential format.
/// </summary>
public class AdminLoginValidator : AbstractValidator<AdminLoginCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminLoginValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    /// <remarks>
    /// Validates email and password presence for admin login attempts.
    /// </remarks>
    public AdminLoginValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Email).ValidEmail(i18n.User.Validation);
        RuleFor(x => x.Password).ValidPassword(i18n.User.Validation, isStrong: false);
    }
}
