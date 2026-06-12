using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Validator for the <see cref="AdminUploadShortVideoThumbnailCommand" /> ensuring a valid short video ID is provided.
/// </summary>
public class AdminUploadShortVideoThumbnailValidator : AbstractValidator<AdminUploadShortVideoThumbnailCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUploadShortVideoThumbnailValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUploadShortVideoThumbnailValidator(ContentI18n i18n)
    {
        RuleFor(x => x.ShortVideoId).IsValidGuid(i18n.ShortVideo.Msg.Localizer);
    }
}
