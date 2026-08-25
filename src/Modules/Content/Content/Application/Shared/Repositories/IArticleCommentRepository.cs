using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Pagination;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for article comments, replies and comment likes.
/// </summary>
public interface IArticleCommentRepository
{
    /// <summary>
    /// Throws when no article row exists, without materializing the aggregate.
    /// </summary>
    /// <param name="articleId">The article identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task ExistsOrThrowAsync(Guid articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a comment record to the repository.
    /// </summary>
    Task AddCommentAsync(ArticleCommentEntity comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of top-level comments for an article (replies excluded),
    /// along with the total top-level count. Soft-deleted comments are included with a null body.
    /// </summary>
    Task<(List<ArticleCommentEntity> Comments, int TotalCount)> GetCommentsAsync(
        Guid articleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns a paginated list of non-deleted replies to a comment, along with total count,
    /// ordered by creation time ascending.
    /// </summary>
    /// <param name="parentCommentId">The parent (top-level) comment identifier.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of replies per page.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The page of replies and the total non-deleted reply count.</returns>
    Task<(List<ArticleCommentEntity> Replies, int TotalCount)> GetRepliesAsync(
        Guid parentCommentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns the number of non-deleted direct replies for each of the given parent comment ids.
    /// Parents with no replies are absent from the result.
    /// </summary>
    /// <param name="parentCommentIds">The parent comment ids to count replies for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A map of parent comment id to its non-deleted reply count.</returns>
    Task<IReadOnlyDictionary<Guid, int>> GetReplyCountsAsync(
        IReadOnlyCollection<Guid> parentCommentIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns a single comment by its ID, or null if not found.
    /// Used by callers that reach a comment without an article in scope.
    /// </summary>
    Task<ArticleCommentEntity?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single comment by its ID scoped to the article it belongs to, or null if no
    /// comment with that ID belongs to that article.
    /// </summary>
    /// <param name="commentId">The comment identifier.</param>
    /// <param name="articleId">The article the comment must belong to.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The comment, or null when it does not belong to the given article.</returns>
    Task<ArticleCommentEntity?> GetCommentByIdAsync(
        Guid commentId,
        Guid articleId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Marks an existing comment as modified.
    /// </summary>
    void UpdateComment(ArticleCommentEntity comment);

    /// <summary>
    /// Returns true if the user has already liked the given comment.
    /// </summary>
    Task<bool> HasLikedCommentAsync(Guid userId, Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a comment-like record to the repository.
    /// </summary>
    Task AddCommentLikeAsync(ArticleCommentLikeEntity like, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the comment-like record for the given user and comment, if present.
    /// </summary>
    Task RemoveCommentLikeAsync(Guid userId, Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the subset of the given comment ids that the viewer has liked. Executes as a
    /// single query; ids the viewer has not liked are absent from the result.
    /// </summary>
    /// <param name="viewerUserId">The current viewer's user id.</param>
    /// <param name="commentIds">The candidate comment ids, typically one page of comments.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The distinct set of input ids the viewer has liked.</returns>
    Task<IReadOnlySet<Guid>> GetLikedCommentIdsAsync(
        Guid viewerUserId,
        IReadOnlyCollection<Guid> commentIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Applies a signed delta to a comment's like counter in a single statement, clamped at zero.
    /// Set-based for the same reason as the article counters.
    /// </summary>
    /// <param name="commentId">The comment whose like counter moves.</param>
    /// <param name="delta">The signed amount to apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>Rows updated; <c>0</c> when the comment no longer exists.</returns>
    Task<int> ApplyCommentLikeDeltaAsync(Guid commentId, int delta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the user's non-deleted comments and replies for one published article.
    /// </summary>
    Task<(List<ArticleCommentEntity> Comments, int TotalCount)> GetOwnCommentsForArticleAsync(
        Guid userId,
        Guid articleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns published articles on which the user has at least one non-deleted comment or reply.
    /// </summary>
    Task<(List<CommentedArticleActivity> Activities, int TotalCount)> GetCommentedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
}
