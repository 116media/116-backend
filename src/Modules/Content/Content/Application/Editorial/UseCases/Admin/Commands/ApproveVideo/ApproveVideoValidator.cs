using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo;

/// <summary>
/// Validator for the <see cref="ApproveVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class ApproveVideoValidator : AbstractValidator<ApproveVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video approval.
    /// </summary>
    public ApproveVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
