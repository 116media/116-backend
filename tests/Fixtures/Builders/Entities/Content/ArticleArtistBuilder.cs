using System.Reflection;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ArticleArtistEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; no factory wraps it yet.
/// </summary>
public class ArticleArtistBuilder
{
    private Guid _id = Guid.NewGuid();
    private ArticleEntity? _article;
    private Guid _artistId = Guid.NewGuid();

    /// <summary>
    /// Sets the artist the article covers.
    /// </summary>
    public ArticleArtistBuilder WithArtistId(Guid artistId)
    {
        _artistId = artistId;
        return this;
    }

    /// <summary>
    /// Adds the credit to the given article, so the join row carries that article's id.
    /// </summary>
    public ArticleArtistBuilder WithArticle(ArticleEntity article)
    {
        _article = article;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="ArticleArtistEntity" /> instance.
    /// </summary>
    public ArticleArtistEntity Build()
    {
        ArticleEntity carrier = _article ?? ArticleFactory.Create(Guid.NewGuid());
        carrier.ReplaceArtists([.. carrier.Artists.Select(credit => credit.ArtistId), _artistId]);

        return carrier.Artists.First(credit => credit.ArtistId == _artistId);
    }
}
