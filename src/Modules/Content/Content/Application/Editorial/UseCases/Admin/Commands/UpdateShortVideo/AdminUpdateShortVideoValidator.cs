using _116.BuildingBlocks.Constants;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;

/// <summary>
/// Validator for the <see cref="AdminUpdateShortVideoCommand" /> ensuring proper update data.
/// </summary>
public class AdminUpdateShortVideoValidator : AbstractValidator<AdminUpdateShortVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateShortVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Short video validation error messages.</param>
    public AdminUpdateShortVideoValidator(ShortVideoErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Short video ID");

        RuleFor(x => x.Title)
            .ValidShortVideoTitle(
                titleRequired: msg.TitleRequired(),
                titleTooLong: msg.TitleTooLong(ContentConstants.MaxShortVideoTitleLength)
            );

        When(
            x => x.VideoFile is not null,
            () =>
            {
                RuleFor(x => x.VideoFile)
                    .ValidShortVideoFile(
                        fileRequired: msg.FileRequired(),
                        fileEmpty: msg.FileEmpty(),
                        fileTooLarge: msg.FileTooLarge(FileConstants.MaxVideoFileSizeBytes / (1024 * 1024)),
                        fileInvalidExtension: msg.FileInvalidExtension(
                            string.Join(", ", FileConstants.AllowedVideoExtensions)
                        )
                    );
            }
        );
    }
}
