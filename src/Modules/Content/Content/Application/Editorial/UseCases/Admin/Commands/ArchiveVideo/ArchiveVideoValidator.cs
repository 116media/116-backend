using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo;

/// <summary>
/// Validator for the <see cref="ArchiveVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class ArchiveVideoValidator : AbstractValidator<ArchiveVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video archiving.
    /// </summary>
    public ArchiveVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
