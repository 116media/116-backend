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

    #region FeaturedArticleSpecification

    [Fact]
    public void FeaturedArticleSpecification_WithFeaturedPublishedArticle_ShouldReturnTrue()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreateFeatured(CategoryId);
        var spec = new FeaturedArticleSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FeaturedArticleSpecification_WithNonFeaturedPublishedArticle_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var spec = new FeaturedArticleSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FeaturedArticleSpecification_WithFeaturedDraftArticle_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.StampFeatured(DateTimeOffset.UtcNow.AddDays(7));
        var spec = new FeaturedArticleSpecification();

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
}
