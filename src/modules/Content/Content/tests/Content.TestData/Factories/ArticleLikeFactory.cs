using _116.Content.Domain.Entities;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="ArticleLikeEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ArticleLikeFactory
{
    /// <summary>
    /// Creates a like record for the given user and article.
    /// </summary>
    public static ArticleLikeEntity Create(Guid userId, Guid articleId) =>
        ArticleLikeEntity.Create(Guid.NewGuid(), userId, articleId);
}
