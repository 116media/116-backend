using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicRateVideoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Stars).ValidVideoStarRating(i18n.ArticleInteraction.Msg);
    }
}
