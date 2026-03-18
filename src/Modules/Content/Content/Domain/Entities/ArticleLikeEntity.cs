using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records that a user has liked an article.
/// Created when a user likes; removed when a user unlikes. Never updated.
/// </summary>
public class ArticleLikeEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who liked the article. No FK to identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The article that was liked.
    /// </summary>
    public Guid ArticleId { get; private set; }

    /// <summary>
    /// Navigation property to the article.
    /// </summary>
    public ArticleEntity Article { get; private set; } = null!;

    private ArticleLikeEntity() { }

    /// <summary>
    /// Creates a new article like record.
    /// </summary>
    /// <param name="id">The unique identifier for this like.</param>
    /// <param name="userId">The user who liked the article.</param>
    /// <param name="articleId">The article that was liked.</param>
    /// <returns>A new <see cref="ArticleLikeEntity" />.</returns>
    public static ArticleLikeEntity Create(Guid id, Guid userId, Guid articleId)
    {
        return new ArticleLikeEntity
        {
            Id = id,
            UserId = userId,
            ArticleId = articleId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
