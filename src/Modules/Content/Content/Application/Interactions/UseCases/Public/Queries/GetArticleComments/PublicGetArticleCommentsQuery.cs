using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments;

/// <summary>
/// Query for retrieving a paginated list of comments for a given article.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article.</param>
/// <param name="PaginatedRequest">Pagination parameters.</param>
/// <param name="ViewerUserId">
/// The current viewer's user ID, or null for an anonymous request. When null, every comment's
/// <c>IsLiked</c> flag resolves to false and no viewer-like query runs.
/// </param>
public record PublicGetArticleCommentsQuery(
    Guid ArticleId,
    PaginatedRequest PaginatedRequest,
    Guid? ViewerUserId = null
) : IQuery<PublicGetArticleCommentsResult>;

/// <summary>
/// Result of the <see cref="PublicGetArticleCommentsQuery" /> containing paginated comments.
/// </summary>
/// <param name="Comments">Paginated result containing article comment DTOs.</param>
public record PublicGetArticleCommentsResult(PaginatedResult<ArticleCommentDto> Comments);
