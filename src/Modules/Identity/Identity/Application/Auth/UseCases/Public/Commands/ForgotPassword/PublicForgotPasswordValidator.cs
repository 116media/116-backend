using _116.Identity.Application.Auth.Validators;

using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;

/// <summary>
/// Validator for the <see cref="PublicForgotPasswordCommand" /> ensuring proper email format.
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
