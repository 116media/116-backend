using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="ArticleCommentEntity"/> instances in tests.
/// </summary>
public static class ArticleCommentFactory
{
    /// <summary>Creates a non-deleted comment on the given article by the given user.</summary>
    public static ArticleCommentEntity Create(Guid articleId, Guid userId) =>
        ArticleCommentEntity.Create(
            id: Guid.NewGuid(),
            userId: userId,
            articleId: articleId,
            body: TestConstants.Content.Interactions.ValidCommentBody
        );

    /// <summary>Creates a non-deleted comment with a specific ID.</summary>
    public static ArticleCommentEntity CreateWithId(Guid id, Guid articleId, Guid userId) =>
        ArticleCommentEntity.Create(
            id: id,
            userId: userId,
            articleId: articleId,
            body: TestConstants.Content.Interactions.ValidCommentBody
        );

    /// <summary>Creates a soft-deleted comment on the given article by the given user.</summary>
    public static ArticleCommentEntity CreateDeleted(Guid articleId, Guid userId)
    {
        ArticleCommentEntity comment = Create(articleId, userId);
        comment.SoftDelete();
        return comment;
    }
}
