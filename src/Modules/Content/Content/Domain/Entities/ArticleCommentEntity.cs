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
    /// Updates the comment body.
    /// </summary>
    /// <param name="body">The new comment text.</param>
    public void Edit(string body) => Body = body;

    /// <summary>
    /// Soft-deletes this comment, hiding its body from public view.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
    }
}
