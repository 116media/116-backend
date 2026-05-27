using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Validator for the <see cref="AdminUploadVideoThumbnailCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminUploadVideoThumbnailValidator : AbstractValidator<AdminUploadVideoThumbnailCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUploadVideoThumbnailValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Video validation error messages.</param>
    public AdminUploadVideoThumbnailValidator(VideoErrorMessage i18n)
    {
        RuleFor(x => x.VideoId).IsValidGuid(i18n.Localizer);
    }
}
