using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentsForArticle;

/// <summary>
/// Retrieves the authenticated user's non-deleted comments and replies for one article.
/// </summary>
public record PublicGetOwnCommentsForArticleQuery(Guid UserId, Guid ArticleId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnCommentsForArticleResult>;

/// <summary>
/// Contains the current user's comments for one published article.
/// </summary>
public record PublicGetOwnCommentsForArticleResult(PaginatedResult<PublicArticleCommentDto> Comments);
