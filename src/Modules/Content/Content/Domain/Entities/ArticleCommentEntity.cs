using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a user comment on an article.
/// Uses Aggregate&lt;Guid&gt; because it has created_at/updated_at and can be edited.
/// </summary>
public class ArticleCommentEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the commenter. No FK to identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The article being commented on.
    /// </summary>
    public Guid ArticleId { get; private set; }

    /// <summary>
    /// The text body of the comment.
    /// </summary>
    public string Body { get; private set; } = null!;

    /// <summary>
    /// Whether this comment has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// When this comment was soft-deleted. Null if not deleted.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <summary>
    /// The parent comment this comment replies to, or null for a top-level comment.
    /// </summary>
    public Guid? ParentCommentId { get; private set; }

    /// <summary>
    /// Navigation to the parent comment, or null for a top-level comment.
    /// </summary>
    public ArticleCommentEntity? ParentComment { get; private set; }

    /// <summary>
    /// Cached number of likes on this comment. Adjusted by like/unlike interactions.
    /// </summary>
    public int LikeCount { get; private set; }

    /// <summary>
    /// Navigation property to the article.
    /// </summary>
    public ArticleEntity Article { get; private set; } = null!;

    private ArticleCommentEntity() { }

    /// <summary>
    /// Creates a new article comment.
    /// </summary>
    /// <param name="id">The unique identifier for the comment.</param>
    /// <param name="userId">The user who posted the comment.</param>
    /// <param name="articleId">The article being commented on.</param>
    /// <param name="body">The comment text.</param>
    /// <returns>A new <see cref="ArticleCommentEntity" />.</returns>
    public static ArticleCommentEntity Create(Guid id, Guid userId, Guid articleId, string body)
    {
        return new ArticleCommentEntity
        {
            Id = id,
            UserId = userId,
            ArticleId = articleId,
            Body = body,
            IsDeleted = false,
        };
    }

    /// <summary>
    /// Creates a reply to an existing top-level comment.
    /// </summary>
    /// <param name="id">The unique identifier for the reply.</param>
    /// <param name="userId">The user who posted the reply.</param>
    /// <param name="articleId">The article being commented on.</param>
    /// <param name="parentCommentId">The top-level comment being replied to.</param>
    /// <param name="body">The reply text.</param>
    /// <returns>A new reply <see cref="ArticleCommentEntity" />.</returns>
    public static ArticleCommentEntity CreateReply(
        Guid id,
        Guid userId,
        Guid articleId,
        Guid parentCommentId,
        string body
    )
    {
        return new ArticleCommentEntity
        {
            Id = id,
            UserId = userId,
            ArticleId = articleId,
            ParentCommentId = parentCommentId,
            Body = body,
            IsDeleted = false,
        };
    }

    /// <summary>
    /// Updates the comment body.
    /// </summary>
    /// <param name="body">The new comment text.</param>
    public void Edit(string body) => Body = body;

    /// <summary>
    /// Increments the cached like count.
    /// </summary>
    public void IncrementLikeCount() => LikeCount++;

    /// <summary>
    /// Decrements the cached like count, never below zero.
    /// </summary>
    public void DecrementLikeCount() => LikeCount = Math.Max(0, LikeCount - 1);

    /// <summary>
    /// Soft-deletes this comment, hiding its body from public view.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
    }
}
