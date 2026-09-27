using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentedArticles;

/// <summary>
/// Retrieves published articles with remaining comments by the authenticated user.
/// </summary>
public record PublicGetOwnCommentedArticlesQuery(Guid UserId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnCommentedArticlesResult>;

/// <summary>
/// Contains the authenticated user's commented article page.
/// </summary>
public record PublicGetOwnCommentedArticlesResult(PaginatedResult<UserCommentedArticleDto> Articles);
