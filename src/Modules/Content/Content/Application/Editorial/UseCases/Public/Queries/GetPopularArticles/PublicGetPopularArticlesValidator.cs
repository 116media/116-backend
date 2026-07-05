using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Validator for the <see cref="PublicGetPopularArticlesQuery" /> ensuring the limit stays
/// within the accepted range.
/// </summary>
public class PublicGetPopularArticlesValidator : AbstractValidator<PublicGetPopularArticlesQuery>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicGetPopularArticlesValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicGetPopularArticlesValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Limit).ValidPopularArticlesLimit(i18n.Article.Msg);
    }
}
