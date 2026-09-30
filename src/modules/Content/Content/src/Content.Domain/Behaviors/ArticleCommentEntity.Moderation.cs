using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Moderation behaviour of <see cref="ArticleCommentEntity" />. Its state lives in <c>Entities/ArticleCommentEntity.cs</c>.
/// </summary>
public partial class ArticleCommentEntity
{
    /// <summary>
    /// Updates the comment body.
    /// </summary>
    /// <param name="body">The new comment text.</param>
    public void Edit(string body) => Body = body;

    /// <summary>
    /// Soft-deletes this comment, hiding its body from public view. Raises
    /// the engagement event so the post-commit consumer decrements the
    /// article's cached comment count.
    /// A no-op when the comment is already soft-deleted: the comment lookups
    /// return deleted rows, so the same comment can be handed to a second
    /// delete (owner delete followed by admin moderation), and a second
    /// <c>-1</c> would drift the article's cached comment count permanently.
    /// </summary>
    /// <returns><c>true</c> if soft-deleted; <c>false</c> if already soft-deleted.</returns>
    public bool SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted)
        {
            return false;
        }

        IsDeleted = true;
        DeletedAt = now;

        AddDomainEvent(new ArticleEngagedEvent(ArticleId: ArticleId, Kind: EnumEngagementKind.Comment, Delta: -1));

        return true;
    }
}
