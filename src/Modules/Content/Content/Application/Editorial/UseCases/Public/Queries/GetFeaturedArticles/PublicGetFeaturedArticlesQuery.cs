using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedArticles;

/// <summary>
/// Query for retrieving the list of currently featured published articles.
/// </summary>
public record PublicGetFeaturedArticlesQuery() : IQuery<PublicGetFeaturedArticlesResult>;

/// <summary>
/// Result of the <see cref="PublicGetFeaturedArticlesQuery" /> containing featured article summaries.
/// </summary>
/// <param name="Articles">The list of featured article summary DTOs.</param>
public record PublicGetFeaturedArticlesResult(IReadOnlyList<ArticleSummaryDto> Articles);
