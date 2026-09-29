using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedArticles;

/// <summary>
/// Retrieves the authenticated user's grouped article share history.
/// </summary>
public record PublicGetOwnSharedArticlesQuery(Guid UserId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnSharedArticlesResult>;

/// <summary>
/// Contains the authenticated user's shared article page.
/// </summary>
public record PublicGetOwnSharedArticlesResult(PaginatedResult<UserArticleActivityDto> Articles);
