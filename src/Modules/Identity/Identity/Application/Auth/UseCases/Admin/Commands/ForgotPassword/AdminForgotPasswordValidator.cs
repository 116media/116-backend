using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;

/// <summary>
/// Validator for the <see cref="AdminForgotPasswordCommand" /> ensuring proper email format.
/// </summary>
public class AdminForgotPasswordValidator : AbstractValidator<AdminForgotPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminForgotPasswordValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    /// <remarks>
    /// Validates email presence and format for admin password reset attempts.
    /// </remarks>
    public AdminForgotPasswordValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Email).ValidEmail(i18n.User.Validation);
    }
}
