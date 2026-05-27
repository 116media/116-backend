using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">Tag validation error messages.</param>
    /// <param name="videoMsg">Video validation error messages.</param>
    public AdminUpdateVideoTagsValidator(TagErrorMessage i18n, VideoErrorMessage videoMsg)
    {
        RuleFor(x => x.VideoId).IsValidGuid(videoMsg.Localizer);
        RuleForEach(x => x.TagNames).ValidTagNameItem(i18n);
    }
}
