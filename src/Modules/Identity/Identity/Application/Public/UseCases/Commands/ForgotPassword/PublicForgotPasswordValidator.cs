using FluentValidation;
using _116.Identity.Application.Shared.Validators;

namespace _116.Identity.Application.Public.UseCases.Commands.ForgotPassword;

/// <summary>
/// Validator for the <see cref="PublicForgotPasswordCommand"/> ensuring proper email format.
/// </summary>
public class PublicForgotPasswordValidator : AbstractValidator<PublicForgotPasswordCommand>
{
    /// <summary>
    /// Configure validation rules for password reset request.
    /// </summary>
    /// <remarks>
    /// Validates email presence and format for password reset attempts.
    /// </remarks>
    public PublicForgotPasswordValidator()
    {
        RuleFor(x => x.Email).EmailValidation();
    }
}
