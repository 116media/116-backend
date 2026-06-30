using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;

/// <summary>
/// Query for retrieving the full details of a published article by its URL slug,
/// including the current user's interaction state.
/// </summary>
/// <param name="Slug">The URL-safe slug of the article to retrieve.</param>
/// <param name="CurrentUserId">
/// The authenticated caller's id, or null for an anonymous request. When null, the per-user
/// interaction flags on the returned DTO resolve to false.
/// </param>
public record PublicGetArticleBySlugQuery(string Slug, Guid? CurrentUserId = null)
    : IQuery<PublicGetArticleBySlugResult>;

/// <summary>
/// Result of the <see cref="PublicGetArticleBySlugQuery" /> containing the full article details.
/// </summary>
/// <param name="Article">The detailed article information.</param>
public record PublicGetArticleBySlugResult(ArticleDetailDto Article);
