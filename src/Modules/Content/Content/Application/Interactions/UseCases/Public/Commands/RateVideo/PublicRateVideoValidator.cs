using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo;

/// <summary>
/// Validator for the <see cref="PublicRateVideoCommand" />.
/// </summary>
public class PublicRateVideoValidator : AbstractValidator<PublicRateVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicRateVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Article interaction validation error messages.</param>
    public PublicRateVideoValidator(ArticleInteractionErrorMessage msg)
    {
        RuleFor(x => x.Stars).ValidVideoStarRating(invalidStarRating: msg.InvalidStarRating());
    }
}
