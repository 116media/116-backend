using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Junction entity linking an article to a tag (many-to-many).
/// </summary>
public class ArticleTagEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identifier of the article.
    /// </summary>
    public Guid ArticleId { get; private set; }

    /// <summary>
    /// The identifier of the tag.
    /// </summary>
    public Guid TagId { get; private set; }

    /// <summary>
    /// The article associated with this tag relationship.
    /// </summary>
    public ArticleEntity Article { get; private set; } = null!;

    /// <summary>
    /// The tag associated with this article relationship.
    /// </summary>
    public TagEntity Tag { get; private set; } = null!;

    private ArticleTagEntity() { }

    /// <summary>
    /// Creates a new article-tag association.
    /// </summary>
    /// <param name="id">The unique identifier for this association.</param>
    /// <param name="articleId">The article being tagged.</param>
    /// <param name="tagId">The tag being applied.</param>
    /// <returns>A new <see cref="ArticleTagEntity" />.</returns>
    public static ArticleTagEntity Create(Guid id, Guid articleId, Guid tagId)
    {
        return new ArticleTagEntity
        {
            Id = id,
            ArticleId = articleId,
            TagId = tagId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
