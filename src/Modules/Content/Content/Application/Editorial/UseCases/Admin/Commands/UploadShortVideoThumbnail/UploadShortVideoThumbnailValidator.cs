using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Validator for the <see cref="UploadShortVideoThumbnailCommand" /> ensuring a valid short video ID is provided.
/// </summary>
public class UploadShortVideoThumbnailValidator : AbstractValidator<UploadShortVideoThumbnailCommand>
{
    /// <summary>
    /// Configures validation rules for short video thumbnail upload.
    /// </summary>
    public UploadShortVideoThumbnailValidator()
    {
        RuleFor(x => x.ShortVideoId).IsValidGuid("Short Video ID");
    }
}
