using FluentValidation;
using _116.Auth.Application.Shared.Validators;

namespace _116.Auth.Application.Public.UseCases.Commands.UpdateAvatar;

/// <summary>
/// Validator for the <see cref="PublicUpdateAvatarCommand"/> ensuring proper avatar URL format.
/// </summary>
/// <remarks>
/// Validates avatar URL according to format requirements:
/// - AvatarUrl: Required, valid URL format, max length according to FileConstants
/// </remarks>
public class PublicUpdateAvatarValidator : AbstractValidator<PublicUpdateAvatarCommand>
{
    /// <summary>
    /// Configure validation rules for public avatar update.
    /// </summary>
    public PublicUpdateAvatarValidator()
    {
        // Avatar URL validation - required for this endpoint
        RuleFor(x => x.AvatarUrl).AvatarUrlValidation(isRequired: true);
    }
}
