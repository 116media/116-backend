using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetCommentReplies;

/// <summary>
/// Query for retrieving a paginated list of replies to a comment.
/// </summary>
/// <param name="CommentId">The parent comment identifier.</param>
/// <param name="PaginatedRequest">Pagination parameters.</param>
/// <param name="ViewerUserId">
/// The current viewer's user ID, or null for an anonymous request. When null, every reply's
/// <c>IsLiked</c> flag resolves to false and no viewer-like query runs.
/// </param>
public record PublicGetCommentRepliesQuery(Guid CommentId, PaginatedRequest PaginatedRequest, Guid? ViewerUserId = null)
    : IQuery<PublicGetCommentRepliesResult>;

/// <summary>
/// Result of the <see cref="PublicGetCommentRepliesQuery" /> containing paginated replies.
/// </summary>
/// <param name="Replies">Paginated reply DTOs.</param>
public record PublicGetCommentRepliesResult(PaginatedResult<ArticleCommentDto> Replies);
