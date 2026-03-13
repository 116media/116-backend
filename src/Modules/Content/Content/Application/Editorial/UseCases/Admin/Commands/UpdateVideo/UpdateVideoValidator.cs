using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Validator for the <see cref="UpdateVideoCommand" /> ensuring valid video update data.
/// </summary>
public class UpdateVideoValidator : AbstractValidator<UpdateVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video updates.
    /// </summary>
    public UpdateVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");

        RuleFor(x => x.Title).ValidVideoTitle();

        RuleFor(x => x.Slug).ValidVideoSlug();
    }
}
