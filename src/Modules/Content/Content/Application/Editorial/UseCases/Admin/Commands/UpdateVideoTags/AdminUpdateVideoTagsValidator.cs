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
    /// Configures validation rules for video tags update.
    /// </summary>
    public AdminUpdateVideoTagsValidator()
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");
        RuleForEach(x => x.TagNames)
            .NotEmpty()
            .WithMessage("Tag name is required.")
            .MaximumLength(ContentConstants.MaxTagNameLength)
            .WithMessage($"Tag name must not exceed {ContentConstants.MaxTagNameLength} characters.");
    }
}
