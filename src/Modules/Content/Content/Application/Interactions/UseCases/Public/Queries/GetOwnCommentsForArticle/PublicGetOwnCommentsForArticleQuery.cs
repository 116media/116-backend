using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentsForArticle;

/// <summary>
/// Retrieves the authenticated user's non-deleted comments and replies for one article.
/// </summary>
public record PublicGetOwnCommentsForArticleQuery(Guid UserId, Guid ArticleId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnCommentsForArticleResult>;

/// <summary>
/// Contains the current user's comments for one published article.
/// </summary>
public record PublicGetOwnCommentsForArticleResult(PaginatedResult<ArticleCommentDto> Comments);
