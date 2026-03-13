using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;

/// <summary>
/// Validator for the <see cref="UpdateVideoTagsCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class UpdateVideoTagsValidator : AbstractValidator<UpdateVideoTagsCommand>
{
    /// <summary>
    /// Configures validation rules for video tags update.
    /// </summary>
    public UpdateVideoTagsValidator()
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");
    }
}
