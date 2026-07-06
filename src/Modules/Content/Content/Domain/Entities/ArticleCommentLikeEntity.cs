using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records that a user has liked an article comment.
/// Created when a user likes; removed when a user unlikes. Never updated.
/// </summary>
public class ArticleCommentLikeEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who liked the comment. No FK to identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The comment that was liked.
    /// </summary>
    public Guid CommentId { get; private set; }

    /// <summary>
    /// Navigation property to the comment.
    /// </summary>
    public ArticleCommentEntity Comment { get; private set; } = null!;

    private ArticleCommentLikeEntity() { }

    /// <summary>
    /// Creates a new comment like record.
    /// </summary>
    /// <param name="id">The unique identifier for this like.</param>
    /// <param name="userId">The user who liked the comment.</param>
    /// <param name="commentId">The comment that was liked.</param>
    /// <returns>A new <see cref="ArticleCommentLikeEntity" />.</returns>
    public static ArticleCommentLikeEntity Create(Guid id, Guid userId, Guid commentId)
    {
        return new ArticleCommentLikeEntity
        {
            Id = id,
            UserId = userId,
            CommentId = commentId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
