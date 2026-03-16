using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Validator for the <see cref="AdminUploadVideoThumbnailCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminUploadVideoThumbnailValidator : AbstractValidator<AdminUploadVideoThumbnailCommand>
{
    /// <summary>
    /// Configures validation rules for video thumbnail upload.
    /// </summary>
    public AdminUploadVideoThumbnailValidator()
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");
    }
}
