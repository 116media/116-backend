using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminCreateShortVideoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Title).ValidShortVideoTitle(i18n.ShortVideo.Msg);

        RuleFor(x => x.Slug).ValidShortVideoSlug(i18n.ShortVideo.Msg);

        RuleFor(x => x.VideoFile).ValidShortVideoFile(i18n.ShortVideo.Msg);
    }
}
