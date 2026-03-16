using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;

/// <summary>
/// Query for retrieving the full details of a published article by its URL slug.
/// </summary>
/// <param name="Slug">The URL-safe slug of the article to retrieve.</param>
public record PublicGetArticleBySlugQuery(string Slug) : IQuery<PublicGetArticleBySlugResult>;

/// <summary>
/// Result of the <see cref="PublicGetArticleBySlugQuery" /> containing the full article details.
/// </summary>
/// <param name="Article">The detailed article information.</param>
public record PublicGetArticleBySlugResult(ArticleDetailDto Article);
