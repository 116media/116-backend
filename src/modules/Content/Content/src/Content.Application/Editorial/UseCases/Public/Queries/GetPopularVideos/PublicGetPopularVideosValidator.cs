using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;

/// <summary>
/// Validator for the <see cref="PublicGetPopularVideosQuery" /> ensuring the limit stays
/// within the accepted range.
/// </summary>
public class PublicGetPopularVideosValidator : AbstractValidator<PublicGetPopularVideosQuery>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicGetPopularVideosValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicGetPopularVideosValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Limit).ValidPopularVideosLimit(i18n.Video.Msg);
    }
}
