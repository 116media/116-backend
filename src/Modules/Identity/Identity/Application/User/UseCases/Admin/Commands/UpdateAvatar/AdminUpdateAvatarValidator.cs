using _116.Identity.Application.Auth.Validators;

using FluentValidation;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;

/// <summary>
/// Validator for the <see cref="AdminUpdateAvatarCommand" /> ensuring proper file constraints.
/// </summary>
/// <remarks>
/// Validates avatar file according to requirements:
/// - AvatarFile: Required, valid image type, max size per FileConstants
/// </remarks>
public class AdminUpdateAvatarValidator : AbstractValidator<AdminUpdateAvatarCommand>
{
    /// <summary>
    /// Configure validation rules for admin avatar update.
    /// </summary>
    public AdminUpdateAvatarValidator()
    {
        // Avatar file validation - required for this endpoint
        RuleFor(x => x.AvatarFile).ValidAvatar(true);
    }
}
