using System.Reflection;
using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ArticleArtistEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; no factory wraps it yet.
/// </summary>
public class ArticleArtistBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _articleId = Guid.NewGuid();
    private Guid _artistId = Guid.NewGuid();
    private ArticleEntity? _article;

    /// <summary>
    /// Sets the artist the article covers.
    /// </summary>
    public ArticleArtistBuilder WithArtistId(Guid artistId)
    {
        _artistId = artistId;
        return this;
    }

    /// <summary>
    /// Attaches the Article navigation EF Core populates through <c>.Include(j =&gt; j.Article)</c>,
    /// and points the foreign key at the same article.
    /// </summary>
    public ArticleArtistBuilder WithArticle(ArticleEntity article)
    {
        _article = article;
        _articleId = article.Id;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="ArticleArtistEntity" /> instance.
    /// </summary>
    public ArticleArtistEntity Build()
    {
        ArticleArtistEntity join = ArticleArtistEntity.Create(_id, _articleId, _artistId);

        if (_article is not null)
        {
            typeof(ArticleArtistEntity)
                .GetProperty(nameof(ArticleArtistEntity.Article), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(join, _article);
        }

        return join;
    }
}
