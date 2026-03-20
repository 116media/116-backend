using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyArticleBookmarks;

/// <summary>
/// Query for retrieving the authenticated user's bookmarked articles.
/// </summary>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
/// <param name="PaginatedRequest">Pagination parameters.</param>
public record PublicGetMyArticleBookmarksQuery(Guid UserId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetMyArticleBookmarksResult>;

/// <summary>
/// Result of the <see cref="PublicGetMyArticleBookmarksQuery" /> containing paginated article summaries.
/// </summary>
/// <param name="Articles">Paginated result containing article summary DTOs.</param>
public record PublicGetMyArticleBookmarksResult(PaginatedResult<ArticleSummaryDto> Articles);
