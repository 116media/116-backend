using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="ArticleArtistEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ArticleArtistFactory
{
    /// <summary>
    /// Credits an artist on the article through the root and returns the junction row.
    /// </summary>
    public static ArticleArtistEntity Link(ArticleEntity article, Guid artistId)
    {
        article.ReplaceArtists([.. article.Artists.Select(credit => credit.ArtistId), artistId]);

        return article.Artists.First(credit => credit.ArtistId == artistId);
    }
}
