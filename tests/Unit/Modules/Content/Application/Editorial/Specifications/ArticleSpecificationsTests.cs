using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for article specification classes.
/// Specifications using EF.Functions.ILike are evaluated through
/// <see cref="ILikeSpecificationEvaluator" />, which rewrites ILike for in-memory execution.
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

    [Theory]
    [InlineData("fally-ipupa-portrait", true)]
    [InlineData("FALLY-IPUPA-PORTRAIT", true)]
    [InlineData("fally-ipupa", false)]
    [InlineData("koffi-olomide", false)]
    public void ArticleBySlugSpecification_ShouldMatchWholeSlugCaseInsensitively(string slug, bool expected)
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreateWithSlug(CategoryId, "fally-ipupa-portrait");
        var spec = new ArticleBySlugSpecification(slug);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(article);

        // Assert
        result.Should().Be(expected);
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

    [Theory]
    [InlineData("fally", true)]
    [InlineData("FALLY IPUPA", true)]
    [InlineData("portrait interview", true)]
    [InlineData("koffi olomide", false)]
    public void ArticleSearchSpecification_ShouldMatchTitleSubstringCaseInsensitively(string search, bool expected)
    {
        // Arrange
        ArticleEntity article = new ArticleBuilder(CategoryId).WithTitle("Fally Ipupa Portrait Interview").Build();
        var spec = new ArticleSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(article);

        // Assert
        result.Should().Be(expected);
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
        ArticleEntity article = new ArticleBuilder(CategoryId).WithCreatedAt(DateTime.UtcNow.AddDays(-7)).Build();
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
        ArticleEntity article = new ArticleBuilder(CategoryId)
            .AsPublished()
            .WithCreatedAt(DateTime.UtcNow.AddDays(-7))
            .Build();
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
        ArticleEntity article = new ArticleBuilder(CategoryId).WithCreatedAt(DateTime.UtcNow).Build();
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

    #region ArticleCommentByIdInArticleSpecification

    [Fact]
    public void ArticleCommentByIdInArticleSpecification_WithMatchingCommentAndArticle_ShouldReturnTrue()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentEntity.Create(Guid.NewGuid(), Guid.NewGuid(), articleId, "body");
        var spec = new ArticleCommentByIdInArticleSpecification(comment.Id, articleId);

        // Act
        bool result = spec.IsSatisfiedBy(comment);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleCommentByIdInArticleSpecification_WithMatchingCommentUnderDifferentArticle_ShouldReturnFalse()
    {
        // Arrange
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "body"
        );
        var spec = new ArticleCommentByIdInArticleSpecification(comment.Id, Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(comment);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ArticleCommentByIdInArticleSpecification_WithDifferentCommentUnderMatchingArticle_ShouldReturnFalse()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentEntity.Create(Guid.NewGuid(), Guid.NewGuid(), articleId, "body");
        var spec = new ArticleCommentByIdInArticleSpecification(Guid.NewGuid(), articleId);

        // Act
        bool result = spec.IsSatisfiedBy(comment);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ArticleCommentByIdInArticleSpecification_WithDifferentCommentAndArticle_ShouldReturnFalse()
    {
        // Arrange
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "body"
        );
        var spec = new ArticleCommentByIdInArticleSpecification(Guid.NewGuid(), Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(comment);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ArticleByArtistSpecification

    [Fact]
    public void ArticleByArtistSpecification_WithPublishedTaggedArticle_ShouldReturnTrue()
    {
        // Arrange
        var artistId = Guid.NewGuid();
        ArticleEntity article = ArticleFactory.CreatePublished(Guid.NewGuid());
        ArticleArtistEntity join = ArticleArtistEntity.Create(Guid.NewGuid(), article.Id, artistId);
        var spec = new ArticleByArtistSpecification(artistId, new[] { join }.AsQueryable());

        // Act & Assert
        spec.ToExpression().Compile()(article).Should().BeTrue();
    }

    [Fact]
    public void ArticleByArtistSpecification_WithDraftTaggedArticle_ShouldReturnFalse()
    {
        // Arrange — the tag exists, but a draft never surfaces publicly.
        var artistId = Guid.NewGuid();
        ArticleEntity draft = ArticleFactory.Create(Guid.NewGuid());
        ArticleArtistEntity join = ArticleArtistEntity.Create(Guid.NewGuid(), draft.Id, artistId);
        var spec = new ArticleByArtistSpecification(artistId, new[] { join }.AsQueryable());

        // Act & Assert
        spec.ToExpression().Compile()(draft).Should().BeFalse();
    }

    [Fact]
    public void ArticleByArtistSpecification_WithUntaggedPublishedArticle_ShouldReturnFalse()
    {
        // Arrange — published, but tagged to nobody.
        ArticleEntity article = ArticleFactory.CreatePublished(Guid.NewGuid());
        var spec = new ArticleByArtistSpecification(Guid.NewGuid(), Array.Empty<ArticleArtistEntity>().AsQueryable());

        // Act & Assert
        spec.ToExpression().Compile()(article).Should().BeFalse();
    }

    #endregion
}
