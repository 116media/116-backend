using _116.Identity.Application.Shared.Validators;

using FluentValidation;

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.SetPassword;

/// <summary>
/// Validator for the <see cref="PublicSetPasswordCommand"/> ensuring proper password format.
/// </summary>
/// <remarks>
/// Validates password according to security requirements:
/// - Password: Strong password with mixed cases, numbers, minimum length
/// </remarks>
public partial class PublicSetPasswordValidator : AbstractValidator<PublicSetPasswordCommand>
{
    /// <summary>
    /// Configure validation rules for Public user password setting.
    /// </summary>
    public PublicSetPasswordValidator()
    {
        // Password validation - strong password requirements
        RuleFor(x => x.Password).PasswordValidation(fieldName: "Password");
    }
}
