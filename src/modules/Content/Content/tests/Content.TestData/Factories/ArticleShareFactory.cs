using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="ArticleShareEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ArticleShareFactory
{
    /// <summary>
    /// Creates a share record attributed to the given user.
    /// </summary>
    public static ArticleShareEntity Create(Guid userId, Guid articleId, EnumShareChannel? shareChannel = null) =>
        ArticleShareEntity.Create(Guid.NewGuid(), userId, articleId, shareChannel);

    /// <summary>
    /// Creates a share record with no signed-in user behind it.
    /// </summary>
    public static ArticleShareEntity CreateAnonymous(Guid articleId, EnumShareChannel? shareChannel = null) =>
        ArticleShareEntity.Create(Guid.NewGuid(), null, articleId, shareChannel);
}
