using _116.Content.Domain.Entities;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="ArticleTagEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ArticleTagFactory
{
    /// <summary>
    /// Creates a tag link between the given article and tag.
    /// </summary>
    public static ArticleTagEntity Create(Guid articleId, Guid tagId) =>
        ArticleTagEntity.Create(Guid.NewGuid(), articleId, tagId);
}
