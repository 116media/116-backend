using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for article specification classes.
/// Note: Specifications using EF.Functions.ILike or cross-type DateTime/DateTimeOffset comparisons
/// require a real PostgreSQL provider — those are covered via ToExpression().Compile() only.
/// </summary>
public class ArticleSpecificationsTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    #region ArticleByIdSpecification

    [Fact]
    public void ArticleByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var spec = new ArticleByIdSpecification(article.Id);

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var spec = new ArticleByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ArticleBySlugSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void ArticleBySlugSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new ArticleBySlugSpecification("fally-ipupa-portrait");

        // Act
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion

    #region ArticleByStatusSpecification

    [Fact]
    public void ArticleByStatusSpecification_WithMatchingStatus_ShouldReturnTrue()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var spec = new ArticleByStatusSpecification(EnumContentStatus.Draft);

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleByStatusSpecification_WithDifferentStatus_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var spec = new ArticleByStatusSpecification(EnumContentStatus.Published);

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ArticleByCategorySpecification

    [Fact]
    public void ArticleByCategorySpecification_WithMatchingCategoryId_ShouldReturnTrue()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var spec = new ArticleByCategorySpecification(CategoryId);

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleByCategorySpecification_WithDifferentCategoryId_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var spec = new ArticleByCategorySpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ArticleSearchSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void ArticleSearchSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new ArticleSearchSpecification("fally");

        // Act
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion

    #region PromotedArticleSpecification

    [Fact]
    public void PromotedArticleSpecification_WithPromotedPublishedArticle_ShouldReturnTrue()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePromoted(CategoryId);
        var spec = new PromotedArticleSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PromotedArticleSpecification_WithNonPromotedPublishedArticle_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var spec = new PromotedArticleSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void PromotedArticleSpecification_WithPromotedDraftArticle_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));
        var spec = new PromotedArticleSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AbandonedDraftSpecification

    [Fact]
    public void AbandonedDraftSpecification_WithDraftArticleCreatedBeforeCutoff_ShouldReturnTrue()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.GetType().GetProperty("CreatedAt")!.SetValue(article, DateTime.UtcNow.AddDays(-7));
        var spec = new AbandonedDraftSpecification(DateTime.UtcNow);

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AbandonedDraftSpecification_WithPublishedArticle_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        article.GetType().GetProperty("CreatedAt")!.SetValue(article, DateTime.UtcNow.AddDays(-7));
        var spec = new AbandonedDraftSpecification(DateTime.UtcNow);

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AbandonedDraftSpecification_WithDraftArticleCreatedAfterCutoff_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.GetType().GetProperty("CreatedAt")!.SetValue(article, DateTime.UtcNow);
        var spec = new AbandonedDraftSpecification(DateTime.UtcNow.AddDays(-1));

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ArticleLikeByUserIdSpecification

    [Fact]
    public void ArticleLikeByUserIdSpecification_WithMatchingUser_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ArticleLikeEntity like = ArticleLikeEntity.Create(Guid.NewGuid(), userId, Guid.NewGuid());
        var spec = new ArticleLikeByUserIdSpecification(userId);

        // Act
        bool result = spec.IsSatisfiedBy(like);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleLikeByUserIdSpecification_WithDifferentUser_ShouldReturnFalse()
    {
        // Arrange
        ArticleLikeEntity like = ArticleLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var spec = new ArticleLikeByUserIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(like);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ArticleShareByUserIdSpecification

    [Fact]
    public void ArticleShareByUserIdSpecification_WithMatchingUser_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ArticleShareEntity share = ArticleShareEntity.Create(Guid.NewGuid(), userId, Guid.NewGuid());
        var spec = new ArticleShareByUserIdSpecification(userId);

        // Act
        bool result = spec.IsSatisfiedBy(share);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleShareByUserIdSpecification_WithDifferentUser_ShouldReturnFalse()
    {
        // Arrange
        ArticleShareEntity share = ArticleShareEntity.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var spec = new ArticleShareByUserIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(share);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ArticleCommentByUserIdSpecification

    [Fact]
    public void ArticleCommentByUserIdSpecification_WithMatchingUser_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentEntity.Create(Guid.NewGuid(), userId, Guid.NewGuid(), "body");
        var spec = new ArticleCommentByUserIdSpecification(userId);

        // Act
        bool result = spec.IsSatisfiedBy(comment);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleCommentByUserIdSpecification_WithDifferentUser_ShouldReturnFalse()
    {
        // Arrange
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "body"
        );
        var spec = new ArticleCommentByUserIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(comment);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ArticleCommentByUserAndArticleSpecification

    [Fact]
    public void ArticleCommentByUserAndArticleSpecification_WithMatchingUserAndArticle_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid articleId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentEntity.Create(Guid.NewGuid(), userId, articleId, "body");
        var spec = new ArticleCommentByUserAndArticleSpecification(userId, articleId);

        // Act
        bool result = spec.IsSatisfiedBy(comment);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleCommentByUserAndArticleSpecification_WithDifferentArticle_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentEntity.Create(Guid.NewGuid(), userId, Guid.NewGuid(), "body");
        var spec = new ArticleCommentByUserAndArticleSpecification(userId, Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(comment);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ArticleCommentByUserAndArticleSpecification_WithDifferentUser_ShouldReturnFalse()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentEntity.Create(Guid.NewGuid(), Guid.NewGuid(), articleId, "body");
        var spec = new ArticleCommentByUserAndArticleSpecification(Guid.NewGuid(), articleId);

        // Act
        bool result = spec.IsSatisfiedBy(comment);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
