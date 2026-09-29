using _116.Content.Domain.Entities;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="ArticleBookmarkEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ArticleBookmarkFactory
{
    /// <summary>
    /// Creates a bookmark record for the given user and article.
    /// </summary>
    public static ArticleBookmarkEntity Create(Guid userId, Guid articleId) =>
        ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, articleId);
}
