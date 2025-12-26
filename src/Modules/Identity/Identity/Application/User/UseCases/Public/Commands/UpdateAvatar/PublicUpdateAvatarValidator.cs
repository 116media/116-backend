using _116.Identity.Application.Auth.Validators;

using FluentValidation;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;

/// <summary>
/// Validator for the <see cref="PublicUpdateAvatarCommand" /> ensuring proper file constraints.
/// </summary>
/// <remarks>
/// Validates avatar file according to requirements:
/// - AvatarFile: Required, valid image type, max size per FileConstants
/// </remarks>
public class PublicUpdateAvatarValidator : AbstractValidator<PublicUpdateAvatarCommand>
{
    /// <summary>
    /// Configure validation rules for public avatar update.
    /// </summary>
    public PublicUpdateAvatarValidator()
    {
        // Avatar file validation - required for this endpoint
        RuleFor(x => x.AvatarFile).AvatarValidation(true);
    }
}
