using _116.BuildingBlocks.Constants;
using _116.User.Domain.Enums;
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
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        // Username validation
        RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(UserConstants.MinUserNameLength)
            .WithMessage($"Username must be at least {UserConstants.MinUserNameLength} characters long");

        // Avatar URL validation (optional)
        RuleFor(x => x.Avatar)
            .Must(BeValidUrlWhenProvided)
            .WithMessage("Avatar must be a valid URL when provided");

        // Provider validation - only Google and Facebook allowed
        RuleFor(x => x.Provider)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Auth provider is required.")
            .Must(provider => provider != null && Enum.IsDefined(typeof(AuthProvider), provider))
            .WithMessage("Auth provider must be Facebook or Google");
    }

    /// <summary>
    /// Validates that avatar is a valid URL when provided.
    /// </summary>
    private static bool BeValidUrlWhenProvided(string? avatar)
    {
        if (string.IsNullOrEmpty(avatar)) return true;

        return Uri.TryCreate(avatar, UriKind.Absolute, out Uri? uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
