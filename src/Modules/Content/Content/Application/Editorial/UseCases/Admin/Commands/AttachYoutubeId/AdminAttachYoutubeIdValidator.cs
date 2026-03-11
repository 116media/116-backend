using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId;

/// <summary>
/// Validator for the <see cref="AdminAttachYoutubeIdCommand" /> ensuring a valid YouTube video ID is provided.
/// </summary>
public class AdminAttachYoutubeIdValidator : AbstractValidator<AdminAttachYoutubeIdCommand>
{
    /// <summary>
    /// Configures validation rules for attaching a YouTube video ID.
    /// </summary>
    public AdminAttachYoutubeIdValidator()
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");

        RuleFor(x => x.YoutubeVideoId).ValidYoutubeVideoId();
    }
}
