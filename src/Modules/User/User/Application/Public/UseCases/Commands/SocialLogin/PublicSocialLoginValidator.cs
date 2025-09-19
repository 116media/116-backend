using _116.User.Domain.Enums;
using _116.User.Application.Shared.Validators;
using FluentValidation;

namespace _116.User.Application.Public.UseCases.Commands.SocialLogin;

/// <summary>
/// Validator for the <see cref="PublicSocialLoginCommand"/> ensuring proper social login data format.
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
    /// Configure validation rules for social login.
    /// </summary>
    public PublicSocialLoginValidator()
    {
        // Email validation
        RuleFor(x => x.Email).EmailValidation();

        // Username validation
        RuleFor(x => x.UserName).UsernameValidation();

        // Avatar URL validation (optional)
        RuleFor(x => x.Avatar)
            .Must(UserValidationRules.BeValidUrl)
            .WithMessage("Avatar must be a valid URL when provided");

        // Provider validation - only Google and Facebook allowed
        RuleFor(x => x.Provider)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Auth provider is required.")
            .Must(provider => provider != null && Enum.IsDefined(typeof(AuthProvider), provider))
            .WithMessage("Auth provider must be Facebook or Google");
    }

}
