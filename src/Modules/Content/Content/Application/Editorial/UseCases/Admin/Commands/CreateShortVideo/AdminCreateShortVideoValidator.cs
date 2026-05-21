using _116.BuildingBlocks.Constants;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Validator for the <see cref="AdminCreateShortVideoCommand" /> ensuring proper short video creation data.
/// </summary>
public class AdminCreateShortVideoValidator : AbstractValidator<AdminCreateShortVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateShortVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Short video validation error messages.</param>
    public AdminCreateShortVideoValidator(ShortVideoErrorMessage msg)
    {
        RuleFor(x => x.Title)
            .ValidShortVideoTitle(
                titleRequired: msg.TitleRequired(),
                titleTooLong: msg.TitleTooLong(ContentConstants.MaxShortVideoTitleLength)
            );

        RuleFor(x => x.Slug)
            .ValidShortVideoSlug(
                slugRequired: msg.SlugRequired(),
                slugTooLong: msg.SlugTooLong(ContentConstants.MaxSlugLength),
                slugInvalidFormat: msg.SlugInvalidFormat()
            );

        RuleFor(x => x.VideoFile)
            .ValidShortVideoFile(
                fileRequired: msg.FileRequired(),
                fileEmpty: msg.FileEmpty(),
                fileTooLarge: msg.FileTooLarge(FileConstants.MaxVideoFileSizeBytes / (1024 * 1024)),
                fileInvalidExtension: msg.FileInvalidExtension(string.Join(", ", FileConstants.AllowedVideoExtensions))
            );
    }
}
