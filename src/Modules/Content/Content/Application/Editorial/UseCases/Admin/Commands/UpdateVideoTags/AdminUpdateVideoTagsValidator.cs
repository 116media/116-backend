using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
    /// <param name="msg">Tag validation error messages.</param>
    public AdminUpdateVideoTagsValidator(TagErrorMessage msg)
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");
        RuleForEach(x => x.TagNames)
            .ValidTagNameItem(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxTagNameLength)
            );
    }
}
