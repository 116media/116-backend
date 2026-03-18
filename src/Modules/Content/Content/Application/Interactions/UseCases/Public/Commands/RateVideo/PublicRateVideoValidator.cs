using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo;

/// <summary>
/// Validator for the <see cref="PublicRateVideoCommand" />.
/// </summary>
public class PublicRateVideoValidator : AbstractValidator<PublicRateVideoCommand>
{
    /// <summary>
    /// Configures validation rules for rating a video.
    /// </summary>
    public PublicRateVideoValidator()
    {
        RuleFor(x => x.Stars).ValidVideoStarRating();
    }
}
