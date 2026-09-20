using _116.Content.Application.Catalog.Specifications;
using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for promotion feed specification classes.
/// </summary>
public class PromotionFeedSpecificationTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    /// <summary>
    /// Populates the PromotionLevel navigation EF Core would load via Include, so the
    /// spot-priority predicates can be evaluated in memory.
    /// </summary>
    private static void AttachPromotionLevel<TEntity>(TEntity entity, PromotionLevelEntity level)
        where TEntity : class
    {
        typeof(TEntity).GetProperty(nameof(ArticleEntity.PromotionLevel))!.SetValue(entity, level);
    }

    #region ArticleBySpotPrioritySpecification

    [Fact]
    public void ArticleBySpotPriority_WhenPromotedPublishedAndSpotMatches_ShouldReturnTrue()
    {
        // Arrange
        PromotionLevelEntity level = new PromotionLevelBuilder().WithSpotPriority(1).Build();
        ArticleEntity article = ArticleFactory.CreatePromoted(CategoryId, level.Id);
        AttachPromotionLevel(article, level);
        var spec = new ArticleBySpotPrioritySpecification(spotPriority: 1);

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArticleBySpotPriority_WhenSpotPriorityDiffers_ShouldReturnFalse()
    {
        // Arrange
        PromotionLevelEntity level = new PromotionLevelBuilder().WithSpotPriority(2).Build();
        ArticleEntity article = ArticleFactory.CreatePromoted(CategoryId, level.Id);
        AttachPromotionLevel(article, level);
        var spec = new ArticleBySpotPrioritySpecification(spotPriority: 1);

        // Act
        bool result = spec.IsSatisfiedBy(article);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ArticleBySpotPriority_WhenNotPromoted_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var spec = new ArticleBySpotPrioritySpecification(spotPriority: 1);
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(article);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ArticleBySpotPriority_WhenDraftArticle_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));
        var spec = new ArticleBySpotPrioritySpecification(spotPriority: 1);
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(article);

        // Assert — IsPromoted = true but Status != Published
        result.Should().BeFalse();
    }

    #endregion

    #region GossipArticleSpecification

    [Theory]
    [InlineData(EnumContentStatus.Published, true)]
    [InlineData(EnumContentStatus.Draft, false)]
    [InlineData(EnumContentStatus.Archived, false)]
    public void GossipArticle_WhenMatchingCategoryAndStatus_ShouldMatchExpected(EnumContentStatus status, bool expected)
    {
        // Arrange
        Guid gossipCategoryId = Guid.NewGuid();
        ArticleEntity article = status switch
        {
            EnumContentStatus.Published => ArticleFactory.CreatePublished(gossipCategoryId),
            EnumContentStatus.Archived => new ArticleBuilder(gossipCategoryId).AsArchived().Build(),
            _ => ArticleFactory.Create(gossipCategoryId),
        };

        var spec = new GossipArticleSpecification(gossipCategoryId);
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(article);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GossipArticle_WhenPublishedButDifferentCategory_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var spec = new GossipArticleSpecification(Guid.NewGuid());
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(article);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region FreeVideoSpecification

    [Fact]
    public void FreeVideo_WhenFreeAndPublished_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        var spec = new FreeVideoSpecification();
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FreeVideo_WhenCommissioned_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = new VideoBuilder(CategoryId)
            .WithCustomer(Guid.NewGuid(), Guid.NewGuid())
            .AsPublished()
            .Build();
        var spec = new FreeVideoSpecification();
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(video);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FreeVideo_WhenFreeButNotPublished_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new FreeVideoSpecification();
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GossipCategorySpecification

    [Fact]
    public void GossipCategory_WhenGossipFallbackAndActive_ShouldReturnTrue()
    {
        // Arrange
        CategoryEntity category = new CategoryBuilder(Guid.NewGuid()).AsGossip().Build();
        var spec = new GossipCategorySpecification();
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(category);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GossipCategory_WhenGossipFallbackButInactive_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = new CategoryBuilder(Guid.NewGuid()).AsGossip().Build();
        category.Deactivate();
        var spec = new GossipCategorySpecification();
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(category);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GossipCategory_WhenNotGossipFallback_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        // IsGossip defaults to false
        var spec = new GossipCategorySpecification();
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(category);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region VideoBySpotPrioritySpecification

    [Fact]
    public void VideoBySpotPriority_WhenPromotedPublishedAndSpotMatches_ShouldReturnTrue()
    {
        // Arrange
        PromotionLevelEntity level = new PromotionLevelBuilder().WithSpotPriority(1).Build();
        VideoEntity video = VideoFactory.CreatePromoted(CategoryId, level.Id);
        AttachPromotionLevel(video, level);
        var spec = new VideoBySpotPrioritySpecification(spotPriority: 1);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VideoBySpotPriority_WhenSpotPriorityDiffers_ShouldReturnFalse()
    {
        // Arrange
        PromotionLevelEntity level = new PromotionLevelBuilder().WithSpotPriority(2).Build();
        VideoEntity video = VideoFactory.CreatePromoted(CategoryId, level.Id);
        AttachPromotionLevel(video, level);
        var spec = new VideoBySpotPrioritySpecification(spotPriority: 1);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VideoBySpotPriority_WhenNotPromoted_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        var spec = new VideoBySpotPrioritySpecification(spotPriority: 1);
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(video);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VideoBySpotPriority_WhenDraftVideo_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));
        var spec = new VideoBySpotPrioritySpecification(spotPriority: 1);
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        // Act
        bool result = predicate(video);

        // Assert — IsPromoted = true but Status != Published
        result.Should().BeFalse();
    }

    #endregion
}
