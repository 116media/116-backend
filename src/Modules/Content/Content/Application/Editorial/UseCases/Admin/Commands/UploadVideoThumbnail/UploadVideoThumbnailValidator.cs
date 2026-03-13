using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Validator for the <see cref="UploadVideoThumbnailCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class UploadVideoThumbnailValidator : AbstractValidator<UploadVideoThumbnailCommand>
{
    /// <summary>
    /// Configures validation rules for video thumbnail upload.
    /// </summary>
    public UploadVideoThumbnailValidator()
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");
    }
}
