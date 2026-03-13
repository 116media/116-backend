using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Validator for the <see cref="SubmitVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class SubmitVideoValidator : AbstractValidator<SubmitVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video submission.
    /// </summary>
    public SubmitVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
