using _116.Identity.Application.Auth.Validators;
using _116.Identity.Domain.Enums;

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
    /// Configure validation rules for social login.
    /// </summary>
    public PublicSocialLoginValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.UserName).ValidUsername();
        RuleFor(x => x.AvatarUrl)
            .Must(predicate: ValidationUtils.ValidUrl)
            .WithMessage("Avatar must be a valid URL when provided");
        RuleFor(x => x.Provider)
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty().WithMessage("Auth provider is required.")
            .Must(provider => provider != null && Enum.IsDefined(typeof(EnumAuthProvider), value: provider))
            .WithMessage("Auth provider must be Facebook or Google");
    }
}
