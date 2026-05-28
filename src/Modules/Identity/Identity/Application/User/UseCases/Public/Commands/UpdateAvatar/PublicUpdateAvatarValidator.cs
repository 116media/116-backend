using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="PublicUpdateAvatarValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicUpdateAvatarValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.AvatarFile).ValidAvatar(i18n, isRequired: true);
    }
}
