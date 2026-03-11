using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Validator for the <see cref="AdminUploadShortVideoThumbnailCommand" /> ensuring a valid short video ID is provided.
/// </summary>
public class AdminUploadShortVideoThumbnailValidator : AbstractValidator<AdminUploadShortVideoThumbnailCommand>
{
    /// <summary>
    /// Configures validation rules for short video thumbnail upload.
    /// </summary>
    public AdminUploadShortVideoThumbnailValidator()
    {
        RuleFor(x => x.ShortVideoId).IsValidGuid("Short Video ID");
    }
}
