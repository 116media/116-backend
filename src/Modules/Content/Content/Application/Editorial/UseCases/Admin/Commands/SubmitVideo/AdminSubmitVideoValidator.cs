using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Validator for the <see cref="AdminSubmitVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminSubmitVideoValidator : AbstractValidator<AdminSubmitVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video submission.
    /// </summary>
    public AdminSubmitVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
