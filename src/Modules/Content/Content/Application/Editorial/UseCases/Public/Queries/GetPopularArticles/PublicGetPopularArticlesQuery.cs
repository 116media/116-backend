using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Query for retrieving the most popular published articles, ranked by a weighted
/// engagement score.
/// </summary>
/// <param name="Limit">
/// Maximum number of articles to return. Validated to a small inclusive range.
/// </param>
/// <param name="CategoryId">
/// Optional category filter. When supplied, only articles in that category are ranked.
/// </param>
/// <param name="ExcludeId">
/// Optional article identifier to omit from the result. Used by the article-detail
/// sidebar to drop the article currently being viewed.
/// </param>
public record PublicGetPopularArticlesQuery(int Limit, Guid? CategoryId, Guid? ExcludeId)
    : IQuery<PublicGetPopularArticlesResult>;

/// <summary>
/// Result of the <see cref="PublicGetPopularArticlesQuery" /> containing the ranked
/// article summaries.
/// </summary>
/// <param name="Articles">The popular articles ordered by engagement score descending.</param>
public record PublicGetPopularArticlesResult(IReadOnlyList<ArticleSummaryDto> Articles);
