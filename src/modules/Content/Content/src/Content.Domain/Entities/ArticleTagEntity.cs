using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Junction entity linking an article to a tag (many-to-many).
/// </summary>
public class ArticleTagEntity : Entity<Guid>
{
    /// <summary>
    /// The identifier of the article.
    /// </summary>
    public Guid ArticleId { get; private set; }

    /// <summary>
    /// The identifier of the tag.
    /// </summary>
    public Guid TagId { get; private set; }

    private ArticleTagEntity() { }

    /// <summary>
    /// Creates a new article-tag association.
    /// </summary>
    /// <param name="id">The unique identifier for this association.</param>
    /// <param name="articleId">The article being tagged.</param>
    /// <param name="tagId">The tag being applied.</param>
    /// <returns>A new <see cref="ArticleTagEntity" />.</returns>
    internal static ArticleTagEntity Create(Guid id, Guid articleId, Guid tagId)
    {
        var association = new ArticleTagEntity
        {
            Id = id,
            ArticleId = articleId,
            TagId = tagId,
        };

        return association;
    }
}
