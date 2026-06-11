using _116.BuildingBlocks.Constants;
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
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicUpdateAvatarValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.AvatarFile)
            .ValidAvatar(
                avatarFileRequired: msg.AvatarFileRequired(),
                avatarFileTooLarge: msg.AvatarFileTooLarge(FileConstants.MaxAvatarFileSizeBytes / (1024 * 1024)),
                avatarFileInvalidType: msg.AvatarFileInvalidType(
                    string.Join(", ", FileConstants.AllowedAvatarMimeTypes)
                ),
                avatarFileInvalidExtension: msg.AvatarFileInvalidExtension(
                    string.Join(", ", FileConstants.AllowedAvatarExtensions)
                ),
                isRequired: true
            );
    }
}
