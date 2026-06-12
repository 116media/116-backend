using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;

/// <summary>
/// Validator for the <see cref="PublicSetPasswordCommand" /> ensuring proper password format.
/// </summary>
/// <remarks>
/// Validates password according to security requirements:
/// - Password: Strong password with mixed cases, numbers, minimum length
/// </remarks>
public class PublicSetPasswordValidator : AbstractValidator<PublicSetPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicSetPasswordValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicSetPasswordValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.Password).ValidPassword(i18n);
    }
}
