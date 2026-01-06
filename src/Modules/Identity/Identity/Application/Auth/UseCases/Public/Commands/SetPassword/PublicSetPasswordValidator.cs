using _116.Identity.Application.Auth.Validators;
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
    /// Configure validation rules for Public user password setting.
    /// </summary>
    public PublicSetPasswordValidator()
    {
        RuleFor(x => x.Password).ValidPassword();
    }
}
