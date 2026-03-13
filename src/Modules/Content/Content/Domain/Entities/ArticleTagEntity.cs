namespace _116.Content.Domain.Entities;

/// <summary>
/// Junction entity linking an article to a tag (many-to-many).
/// Uses a composite primary key configured in EF Core: (ArticleId, TagId).
/// </summary>
public class ArticleTagEntity
{
    /// <summary>
    /// The identifier of the article.
    /// </summary>
    public Guid ArticleId { get; set; }

    /// <summary>
    /// The identifier of the tag.
    /// </summary>
    public Guid TagId { get; set; }

    /// <summary>
    /// The article associated with this tag relationship.
    /// </summary>
    public ArticleEntity Article { get; set; } = null!;

    /// <summary>
    /// The tag associated with this article relationship.
    /// </summary>
    public TagEntity Tag { get; set; } = null!;
}
