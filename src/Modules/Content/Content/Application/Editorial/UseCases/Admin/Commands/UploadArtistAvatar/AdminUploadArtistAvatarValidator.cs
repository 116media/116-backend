using _116.Content.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar;

/// <summary>
/// Validator for the <see cref="AdminUploadArtistAvatarCommand" /> ensuring an avatar image file is provided.
/// </summary>
public class AdminUploadArtistAvatarValidator : AbstractValidator<AdminUploadArtistAvatarCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUploadArtistAvatarValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUploadArtistAvatarValidator(ContentI18n i18n)
    {
        RuleFor(x => x.File).NotNull().WithMessage(i18n.Lyrics.Msg.FileRequired());
    }
}
