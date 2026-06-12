using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
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
    /// Initializes a new instance of <see cref="AdminUpdateAvatarValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminUpdateAvatarValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.AvatarFile).ValidAvatar(i18n.User.Validation, isRequired: true);
    }
}
