using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateShortVideoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.ShortVideo.Msg.Localizer);

        RuleFor(x => x.Title).ValidShortVideoTitle(i18n.ShortVideo.Msg);

        When(
            x => x.VideoFile is not null,
            () =>
            {
                RuleFor(x => x.VideoFile).ValidShortVideoFile(i18n.ShortVideo.Msg);
            }
        );
    }
}
