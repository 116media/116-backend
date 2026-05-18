using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;

/// <summary>
/// Validator for the <see cref="AdminAttachYoutubeVideoUrlCommand" /> ensuring a valid YouTube video URL is provided.
/// </summary>
public class AdminAttachYoutubeVideoUrlValidator : AbstractValidator<AdminAttachYoutubeVideoUrlCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminAttachYoutubeVideoUrlValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Video validation error messages.</param>
    public AdminAttachYoutubeVideoUrlValidator(VideoErrorMessage msg)
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");

        RuleFor(x => x.YoutubeVideoUrl).ValidYoutubeVideoUrl(msg);
    }
}
