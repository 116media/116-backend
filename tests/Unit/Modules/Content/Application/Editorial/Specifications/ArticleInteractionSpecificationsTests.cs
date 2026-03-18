using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for the new article interaction and sub-entity specification classes.
/// </summary>
public class ArticleInteractionSpecificationsTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    #region ArticleByOrderItemIdSpecification

    [Fact]
    public void ArticleByOrderItemIdSpecification_WithMatchingOrderItemId_ShouldReturnTrue()
    {
        Guid orderItemId = Guid.NewGuid();
        ArticleEntity article = ArticleFactory.CreatePaid(CategoryId, Guid.NewGuid(), orderItemId);
        var spec = new ArticleByOrderItemIdSpecification(orderItemId);

        bool result = spec.IsSatisfiedBy(article);

        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleByOrderItemIdSpecification_WithDifferentOrderItemId_ShouldReturnFalse()
    {
        ArticleEntity article = ArticleFactory.CreatePaid(CategoryId, Guid.NewGuid(), Guid.NewGuid());
        var spec = new ArticleByOrderItemIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(article);

        result.Should().BeFalse();
    }

    #endregion

    #region ArticleImageByArticleIdSpecification

    [Fact]
    public void ArticleImageByArticleIdSpecification_WithMatchingArticleId_ShouldReturnTrue()
    {
        Guid articleId = Guid.NewGuid();
        ArticleImageEntity image = ArticleImageFactory.Create(articleId);
        var spec = new ArticleImageByArticleIdSpecification(articleId);

        bool result = spec.IsSatisfiedBy(image);

        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleImageByArticleIdSpecification_WithDifferentArticleId_ShouldReturnFalse()
    {
        ArticleImageEntity image = ArticleImageFactory.Create(Guid.NewGuid());
        var spec = new ArticleImageByArticleIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(image);

        result.Should().BeFalse();
    }

    #endregion

    #region ArticleTagByArticleIdSpecification

    [Fact]
    public void ArticleTagByArticleIdSpecification_WithMatchingArticleId_ShouldReturnTrue()
    {
        Guid articleId = Guid.NewGuid();
        ArticleTagEntity tag = ArticleTagEntity.Create(Guid.NewGuid(), articleId: articleId, tagId: Guid.NewGuid());
        var spec = new ArticleTagByArticleIdSpecification(articleId);

        bool result = spec.IsSatisfiedBy(tag);

        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleTagByArticleIdSpecification_WithDifferentArticleId_ShouldReturnFalse()
    {
        ArticleTagEntity tag = ArticleTagEntity.Create(
            Guid.NewGuid(),
            articleId: Guid.NewGuid(),
            tagId: Guid.NewGuid()
        );
        var spec = new ArticleTagByArticleIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(tag);

        result.Should().BeFalse();
    }

    #endregion

    #region ArticleLikeByUserAndArticleSpecification

    [Fact]
    public void ArticleLikeByUserAndArticleSpecification_WithMatchingUserAndArticle_ShouldReturnTrue()
    {
        Guid userId = Guid.NewGuid();
        Guid articleId = Guid.NewGuid();
        ArticleLikeEntity like = ArticleLikeEntity.Create(Guid.NewGuid(), userId: userId, articleId: articleId);
        var spec = new ArticleLikeByUserAndArticleSpecification(userId, articleId);

        bool result = spec.IsSatisfiedBy(like);

        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleLikeByUserAndArticleSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        ArticleLikeEntity like = ArticleLikeEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            articleId: Guid.NewGuid()
        );
        var spec = new ArticleLikeByUserAndArticleSpecification(Guid.NewGuid(), like.ArticleId);

        bool result = spec.IsSatisfiedBy(like);

        result.Should().BeFalse();
    }

    [Fact]
    public void ArticleLikeByUserAndArticleSpecification_WithDifferentArticleId_ShouldReturnFalse()
    {
        Guid userId = Guid.NewGuid();
        ArticleLikeEntity like = ArticleLikeEntity.Create(Guid.NewGuid(), userId: userId, articleId: Guid.NewGuid());
        var spec = new ArticleLikeByUserAndArticleSpecification(userId, Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(like);

        result.Should().BeFalse();
    }

    #endregion

    #region ArticleBookmarkByUserAndArticleSpecification

    [Fact]
    public void ArticleBookmarkByUserAndArticleSpecification_WithMatchingUserAndArticle_ShouldReturnTrue()
    {
        Guid userId = Guid.NewGuid();
        Guid articleId = Guid.NewGuid();
        ArticleBookmarkEntity bookmark = ArticleBookmarkEntity.Create(
            Guid.NewGuid(),
            userId: userId,
            articleId: articleId
        );
        var spec = new ArticleBookmarkByUserAndArticleSpecification(userId, articleId);

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleBookmarkByUserAndArticleSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        ArticleBookmarkEntity bookmark = ArticleBookmarkEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            articleId: Guid.NewGuid()
        );
        var spec = new ArticleBookmarkByUserAndArticleSpecification(Guid.NewGuid(), bookmark.ArticleId);

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeFalse();
    }

    [Fact]
    public void ArticleBookmarkByUserAndArticleSpecification_WithDifferentArticleId_ShouldReturnFalse()
    {
        Guid userId = Guid.NewGuid();
        ArticleBookmarkEntity bookmark = ArticleBookmarkEntity.Create(
            Guid.NewGuid(),
            userId: userId,
            articleId: Guid.NewGuid()
        );
        var spec = new ArticleBookmarkByUserAndArticleSpecification(userId, Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeFalse();
    }

    #endregion

    #region ArticleBookmarkByUserIdSpecification

    [Fact]
    public void ArticleBookmarkByUserIdSpecification_WithMatchingUserId_ShouldReturnTrue()
    {
        Guid userId = Guid.NewGuid();
        ArticleBookmarkEntity bookmark = ArticleBookmarkEntity.Create(
            Guid.NewGuid(),
            userId: userId,
            articleId: Guid.NewGuid()
        );
        var spec = new ArticleBookmarkByUserIdSpecification(userId);

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleBookmarkByUserIdSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        ArticleBookmarkEntity bookmark = ArticleBookmarkEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            articleId: Guid.NewGuid()
        );
        var spec = new ArticleBookmarkByUserIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeFalse();
    }

    #endregion

    #region ArticleCommentByArticleIdSpecification

    [Fact]
    public void ArticleCommentByArticleIdSpecification_WithMatchingArticleId_ShouldReturnTrue()
    {
        Guid articleId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            articleId: articleId,
            body: "test"
        );
        var spec = new ArticleCommentByArticleIdSpecification(articleId);

        bool result = spec.IsSatisfiedBy(comment);

        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleCommentByArticleIdSpecification_WithDifferentArticleId_ShouldReturnFalse()
    {
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            articleId: Guid.NewGuid(),
            body: "test"
        );
        var spec = new ArticleCommentByArticleIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(comment);

        result.Should().BeFalse();
    }

    #endregion

    #region ArticleCommentByIdSpecification

    [Fact]
    public void ArticleCommentByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        Guid commentId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            commentId,
            userId: Guid.NewGuid(),
            articleId: Guid.NewGuid(),
            body: "test"
        );
        var spec = new ArticleCommentByIdSpecification(commentId);

        bool result = spec.IsSatisfiedBy(comment);

        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleCommentByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            articleId: Guid.NewGuid(),
            body: "test"
        );
        var spec = new ArticleCommentByIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(comment);

        result.Should().BeFalse();
    }

    #endregion
}
