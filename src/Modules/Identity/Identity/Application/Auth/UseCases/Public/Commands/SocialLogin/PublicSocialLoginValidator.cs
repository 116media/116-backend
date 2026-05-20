using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Validator for the <see cref="PublicSocialLoginCommand" /> ensuring proper social login data format.
/// </summary>
/// <remarks>
/// Validates email, username, avatar URL, and authentication provider according to requirements:
/// - Email: Valid email format and required
/// - UserName: Required, minimum length validation
/// - Avatar: Optional, valid URL format if provided
/// - Provider: Must be Google or Facebook only
/// </remarks>
public class PublicSocialLoginValidator : AbstractValidator<PublicSocialLoginCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicSocialLoginValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicSocialLoginValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Email).ValidEmail(msg);
        RuleFor(x => x.UserName).ValidUsername(msg);
        RuleFor(x => x.AvatarUrl).ValidAvatarUrl(msg);
        RuleFor(x => x.Provider).ValidAuthProvider(msg);
    }
}
