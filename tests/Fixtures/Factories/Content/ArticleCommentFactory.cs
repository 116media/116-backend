using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="ArticleCommentEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ArticleCommentFactory
{
    /// <summary>
    /// Creates a non-deleted comment on the given article by the given user.
    /// </summary>
    public static ArticleCommentEntity Create(Guid articleId, Guid userId) =>
        ArticleCommentEntity.Create(
            id: Guid.NewGuid(),
            userId: userId,
            articleId: articleId,
            body: TestConstants.Interactions.ValidCommentBody
        );

    /// <summary>
    /// Creates a soft-deleted comment on the given article by the given user.
    /// </summary>
    public static ArticleCommentEntity CreateDeleted(Guid articleId, Guid userId)
    {
        ArticleCommentEntity comment = Create(articleId, userId);
        comment.SoftDelete();
        return comment;
    }
}
