using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;

/// <summary>
/// Validator for the <see cref="PublicForgotPasswordCommand" /> ensuring proper email format.
/// </summary>
/// <remarks>
/// Validates email presence and format for password reset attempts.
/// </remarks>
public class PublicForgotPasswordValidator : AbstractValidator<PublicForgotPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicForgotPasswordValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicForgotPasswordValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.Email).ValidEmail(i18n);
    }
}
