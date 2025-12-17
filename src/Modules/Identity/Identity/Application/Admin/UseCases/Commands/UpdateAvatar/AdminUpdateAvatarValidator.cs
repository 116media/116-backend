using FluentValidation;
using _116.Identity.Application.Shared.Validators;

namespace _116.Identity.Application.Admin.UseCases.Commands.UpdateAvatar;

/// <summary>
/// Validator for the <see cref="AdminUpdateAvatarCommand"/> ensuring proper file constraints.
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
        RuleFor(x => x.AvatarFile).AvatarValidation(isRequired: true);
    }
}
