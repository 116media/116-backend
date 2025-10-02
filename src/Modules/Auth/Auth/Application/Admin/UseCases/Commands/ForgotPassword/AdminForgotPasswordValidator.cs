using FluentValidation;
using _116.Auth.Application.Shared.Validators;

namespace _116.Auth.Application.Admin.UseCases.Commands.ForgotPassword;

/// <summary>
/// Validator for the <see cref="AdminForgotPasswordCommand"/> ensuring proper email format.
/// </summary>
public class AdminForgotPasswordValidator : AbstractValidator<AdminForgotPasswordCommand>
{
    /// <summary>
    /// Configure validation rules for admin password reset request.
    /// </summary>
    /// <remarks>
    /// Validates email presence and format for admin password reset attempts.
    /// </remarks>
    public AdminForgotPasswordValidator()
    {
        RuleFor(x => x.Email).EmailValidation();
    }
}
