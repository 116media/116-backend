using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminAttachYoutubeVideoUrlValidator(ContentI18n i18n)
    {
        RuleFor(x => x.VideoId).IsValidGuid(i18n.Video.Msg.Localizer);

        RuleFor(x => x.YoutubeVideoUrl).ValidYoutubeVideoUrl(i18n.Video.Msg);
    }
}
