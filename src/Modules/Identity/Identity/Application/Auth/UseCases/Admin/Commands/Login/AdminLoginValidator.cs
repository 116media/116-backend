using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    /// <remarks>
    /// Validates email and password presence for admin login attempts.
    /// </remarks>
    public AdminLoginValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Email)
            .ValidEmail(
                emailRequired: msg.EmailRequired(),
                emailTooLong: msg.EmailTooLong(UserConstants.MaxEmailLength),
                invalidEmailFormat: msg.InvalidEmailFormatMsg()
            );
        RuleFor(x => x.Password).ValidPassword(passwordRequired: msg.PasswordRequired(), isStrong: false);
    }
}
