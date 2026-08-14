using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Junction entity linking an article to an artist it covers (many-to-many).
/// <para>
/// A join table rather than a single FK on the article: an article routinely covers several
/// artists, and a single FK would force an arbitrary choice of which profile gets the story.
/// </para>
/// </summary>
public class ArticleArtistEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identifier of the article.
    /// </summary>
    public Guid ArticleId { get; private set; }

    /// <summary>
    /// The identifier of the artist the article covers.
    /// </summary>
    public Guid ArtistId { get; private set; }

    /// <summary>
    /// The article associated with this artist relationship.
    /// </summary>
    public ArticleEntity Article { get; private set; } = null!;

    /// <summary>
    /// The artist associated with this article relationship.
    /// </summary>
    public ArtistEntity Artist { get; private set; } = null!;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ArticleArtistEntity() { }

    /// <summary>
    /// Creates a new article-artist association.
    /// </summary>
    /// <param name="id">The unique identifier for this association.</param>
    /// <param name="articleId">The article being tagged.</param>
    /// <param name="artistId">The artist the article covers.</param>
    /// <returns>A new <see cref="ArticleArtistEntity" />.</returns>
    public static ArticleArtistEntity Create(Guid id, Guid articleId, Guid artistId)
    {
        return new ArticleArtistEntity
        {
            Id = id,
            ArticleId = articleId,
            ArtistId = artistId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
