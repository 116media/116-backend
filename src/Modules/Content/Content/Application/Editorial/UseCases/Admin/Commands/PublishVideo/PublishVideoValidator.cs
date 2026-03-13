using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo;

/// <summary>
/// Validator for the <see cref="PublishVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class PublishVideoValidator : AbstractValidator<PublishVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video publishing.
    /// </summary>
    public PublishVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
