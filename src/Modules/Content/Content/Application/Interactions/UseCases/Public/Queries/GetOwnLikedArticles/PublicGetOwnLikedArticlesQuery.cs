using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnLikedArticles;

/// <summary>
/// Retrieves the authenticated user's currently liked published articles.
/// </summary>
public record PublicGetOwnLikedArticlesQuery(Guid UserId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnLikedArticlesResult>;

/// <summary>
/// Contains the authenticated user's liked article page.
/// </summary>
public record PublicGetOwnLikedArticlesResult(PaginatedResult<UserArticleActivityDto> Articles);
