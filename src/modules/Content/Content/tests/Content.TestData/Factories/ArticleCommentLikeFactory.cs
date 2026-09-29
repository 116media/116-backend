using _116.Content.Domain.Entities;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="ArticleCommentLikeEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ArticleCommentLikeFactory
{
    /// <summary>
    /// Creates a like record for the given user and comment.
    /// </summary>
    public static ArticleCommentLikeEntity Create(Guid userId, Guid commentId) =>
        ArticleCommentLikeEntity.Create(Guid.NewGuid(), userId, commentId);
}
