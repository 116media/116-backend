using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Validator for the <see cref="UpdateVideoSeoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class UpdateVideoSeoValidator : AbstractValidator<UpdateVideoSeoCommand>
{
    /// <summary>
    /// Configures validation rules for video SEO update.
    /// </summary>
    public UpdateVideoSeoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
