using FluentValidation;
using _116.Auth.Application.Shared.Validators;

namespace _116.Auth.Application.Admin.UseCases.Commands.UpdateAvatar;

/// <summary>
/// Validator for the <see cref="AdminUpdateAvatarCommand"/> ensuring proper avatar URL format.
/// </summary>
/// <remarks>
/// Validates avatar URL according to format requirements:
/// - AvatarUrl: Required, valid URL format, max length according to FileConstants
/// </remarks>
public class AdminUpdateAvatarValidator : AbstractValidator<AdminUpdateAvatarCommand>
{
    /// <summary>
    /// Configure validation rules for admin avatar update.
    /// </summary>
    public AdminUpdateAvatarValidator()
    {
        // Avatar URL validation - required for this endpoint
        RuleFor(x => x.AvatarUrl).AvatarUrlValidation(isRequired: true);
    }
}
