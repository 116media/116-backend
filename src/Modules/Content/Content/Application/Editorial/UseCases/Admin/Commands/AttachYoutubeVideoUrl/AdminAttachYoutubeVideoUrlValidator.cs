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
    /// Configures validation rules for attaching a YouTube video Url.
    /// </summary>
    public AdminAttachYoutubeVideoUrlValidator()
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");

        RuleFor(x => x.YoutubeVideoUrl).ValidYoutubeVideoUrl();
    }
}
