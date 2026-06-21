using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;

/// <summary>
/// Validator for the <see cref="AdminUploadShortVideoFileCommand" /> ensuring a valid short video ID
/// and a valid video file are provided.
/// </summary>
public class AdminUploadShortVideoFileValidator : AbstractValidator<AdminUploadShortVideoFileCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUploadShortVideoFileValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUploadShortVideoFileValidator(ContentI18n i18n)
    {
        RuleFor(x => x.ShortVideoId).IsValidGuid(i18n.ShortVideo.Msg.Localizer);

        RuleFor(x => x.File).ValidShortVideoFile(i18n.ShortVideo.Msg);
    }
}
