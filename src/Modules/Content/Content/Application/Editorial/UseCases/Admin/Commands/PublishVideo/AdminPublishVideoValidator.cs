using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo;

/// <summary>
/// Validator for the <see cref="AdminPublishVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminPublishVideoValidator : AbstractValidator<AdminPublishVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video publishing.
    /// </summary>
    public AdminPublishVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
