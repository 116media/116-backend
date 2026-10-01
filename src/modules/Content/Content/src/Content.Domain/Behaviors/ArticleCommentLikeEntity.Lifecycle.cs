using _116.Content.Domain.Events;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="ArticleCommentLikeEntity" />. Its state lives in <c>Entities/ArticleCommentLikeEntity.cs</c>.
/// </summary>
public partial class ArticleCommentLikeEntity
{
    /// <summary>
    /// Declares this like's removal so the post-commit engagement consumer
    /// can decrement the comment's cached like count.
    /// Called by the removal path immediately before the row is removed.
    /// </summary>
    public void MarkRemoved()
    {
        AddDomainEvent(new CommentEngagedEvent(CommentId: CommentId, Delta: -1));
    }
}
