using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records that a user (or anonymous visitor) shared an article.
/// UserId is nullable — anonymous social shares are tracked too.
/// Uses a regular UUID primary key (not composite) because UserId can be null.
/// </summary>
public class ArticleShareEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who shared. Null for anonymous shares.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// The article that was shared.
    /// </summary>
    public Guid ArticleId { get; private set; }

    /// <summary>
    /// Navigation property to the article.
    /// </summary>
    public ArticleEntity Article { get; private set; } = null!;

    private ArticleShareEntity() { }

    /// <summary>
    /// Creates a new article share record.
    /// </summary>
    /// <param name="id">The unique identifier for this share.</param>
    /// <param name="userId">The user who shared. Null for anonymous shares.</param>
    /// <param name="articleId">The article that was shared.</param>
    /// <returns>A new <see cref="ArticleShareEntity" />.</returns>
    public static ArticleShareEntity Create(Guid id, Guid? userId, Guid articleId)
    {
        return new ArticleShareEntity
        {
            Id = id,
            UserId = userId,
            ArticleId = articleId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
