using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records that a user has bookmarked an article.
/// Created when a user bookmarks; removed when a user un-bookmarks. Never updated.
/// </summary>
public class ArticleBookmarkEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who bookmarked the article. No FK to identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The article that was bookmarked.
    /// </summary>
    public Guid ArticleId { get; private set; }

    /// <summary>
    /// Navigation property to the article.
    /// </summary>
    public ArticleEntity Article { get; private set; } = null!;

    private ArticleBookmarkEntity() { }

    /// <summary>
    /// Creates a new article bookmark record.
    /// </summary>
    /// <param name="id">The unique identifier for this bookmark.</param>
    /// <param name="userId">The user who bookmarked the article.</param>
    /// <param name="articleId">The article that was bookmarked.</param>
    /// <returns>A new <see cref="ArticleBookmarkEntity" />.</returns>
    public static ArticleBookmarkEntity Create(Guid id, Guid userId, Guid articleId)
    {
        return new ArticleBookmarkEntity
        {
            Id = id,
            UserId = userId,
            ArticleId = articleId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
