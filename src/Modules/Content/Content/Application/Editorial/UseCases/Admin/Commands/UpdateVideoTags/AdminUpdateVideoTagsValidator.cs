using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;

/// <summary>
/// Validator for the <see cref="AdminUpdateVideoTagsCommand" /> ensuring a valid video ID
/// and that each tag name satisfies the tag name constraints.
/// </summary>
public class AdminUpdateVideoTagsValidator : AbstractValidator<AdminUpdateVideoTagsCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateVideoTagsValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateVideoTagsValidator(ContentI18n i18n)
    {
        RuleFor(x => x.VideoId).IsValidGuid(i18n.Video.Msg.Localizer);
        RuleForEach(x => x.TagNames).ValidTagNameItem(i18n.Tag.Msg);
    }
}
