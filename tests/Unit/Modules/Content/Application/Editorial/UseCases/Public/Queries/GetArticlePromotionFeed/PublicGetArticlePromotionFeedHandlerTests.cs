using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed;

/// <summary>
/// Unit tests for <see cref="PublicGetArticlePromotionFeedHandler"/>.
/// </summary>
public class PublicGetArticlePromotionFeedHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetArticlePromotionFeedHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetArticlePromotionFeedHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        FileEntity coverFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupGetById(coverFile);
        _handler = new PublicGetArticlePromotionFeedHandler(
            _articleRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    #region All spots filled

    [Fact]
    public async Task Handle_WhenAllSpotsHavePromotedArticles_ShouldReturnPromotedArticlesWithNoFallbacks()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());
        List<ArticleEntity> spot1 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> spot2 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> spot3 = ArticleFactory.CreateManyPublished(CategoryId, 2);
        List<ArticleEntity> gossipPool = ArticleFactory.CreateManyPublished(CategoryId, 5);

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, spot1);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, spot2);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, spot3);
        _articleRepositoryMock.SetupGetGossipFallback(gossipPool);

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot1.Articles.Should().ContainSingle();
        result.Spot2.Articles.Should().ContainSingle();
        result.Spot3.Slots.Should().HaveCount(2);
        result.GossipStrip.Should().HaveCount(3);
    }

    #endregion

    #region Spot 1 empty — fallback

    [Fact]
    public async Task Handle_WhenSpot1Empty_ShouldFallBackToOneGossipArticle()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());
        List<ArticleEntity> spot2 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> spot3 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> gossipPool = ArticleFactory.CreateManyPublished(CategoryId, 5);

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, spot2);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, spot3);
        _articleRepositoryMock.SetupGetGossipFallback(gossipPool);

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot1.SpotPriority.Should().Be(1);
        result.Spot1.Articles.Should().ContainSingle();
        result.Spot2.Articles.Should().ContainSingle();
    }

    #endregion

    #region Spot 2 empty — fallback

    [Fact]
    public async Task Handle_WhenSpot2Empty_ShouldFallBackToOneGossipArticle()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());
        List<ArticleEntity> spot1 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> spot3 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> gossipPool = ArticleFactory.CreateManyPublished(CategoryId, 5);

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, spot1);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, spot3);
        _articleRepositoryMock.SetupGetGossipFallback(gossipPool);

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot2.SpotPriority.Should().Be(2);
        result.Spot2.Articles.Should().ContainSingle();
    }

    #endregion

    #region Spot 3 partial — one promoted, one fallback

    [Fact]
    public async Task Handle_WhenSpot3HasOnePromo_ShouldPutItInColumnAAndFallbackInColumnB()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());
        List<ArticleEntity> spot3 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> gossipPool = ArticleFactory.CreateManyPublished(CategoryId, 5);

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, spot3);
        _articleRepositoryMock.SetupGetGossipFallback(gossipPool);

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        ArticlePromotionSlotDto slotA = result.Spot3.Slots.Single(s => s.Position == "a");
        ArticlePromotionSlotDto slotB = result.Spot3.Slots.Single(s => s.Position == "b");
        slotA.Articles.Should().ContainSingle();
        slotB.Articles.Should().ContainSingle();
    }

    #endregion

    #region Spot 3 fully empty — both columns get fallbacks

    [Fact]
    public async Task Handle_WhenSpot3FullyEmpty_ShouldFillBothColumnsWithGossipFallback()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());
        List<ArticleEntity> gossipPool = ArticleFactory.CreateManyPublished(CategoryId, 10);

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetGossipFallback(gossipPool);

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        ArticlePromotionSlotDto slotA = result.Spot3.Slots.Single(s => s.Position == "a");
        ArticlePromotionSlotDto slotB = result.Spot3.Slots.Single(s => s.Position == "b");
        slotA.Articles.Should().ContainSingle();
        slotB.Articles.Should().ContainSingle();
    }

    #endregion

    #region All spots empty

    [Fact]
    public async Task Handle_WhenAllSpotsEmpty_ShouldFillAllFromGossipPool()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());
        List<ArticleEntity> gossipPool = ArticleFactory.CreateManyPublished(CategoryId, 10);

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetGossipFallback(gossipPool);

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot1.Articles.Should().ContainSingle();
        result.Spot2.Articles.Should().ContainSingle();
        result.Spot3.SpotPriority.Should().Be(3);
        result.GossipStrip.Count.Should().BeLessThanOrEqualTo(3);
    }

    #endregion

    #region Gossip pool exhausted

    [Fact]
    public async Task Handle_WhenGossipPoolEmpty_ShouldReturnEmptySpotsWithNoFallback()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetGossipFallback(new List<ArticleEntity>());

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot1.Articles.Should().BeEmpty();
        result.Spot2.Articles.Should().BeEmpty();
        result.GossipStrip.Should().BeEmpty();
    }

    #endregion

    #region No gossip category configured

    [Fact]
    public async Task Handle_WhenNoGossipCategoryExists_ShouldReturnEmptyFallbacksAndStrip()
    {
        // Arrange — gossip category is null (not configured)
        _categoryRepositoryMock.SetupGetGossipCategory(null);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, new List<ArticleEntity>());

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot1.Articles.Should().BeEmpty();
        result.Spot2.Articles.Should().BeEmpty();
        result.GossipStrip.Should().BeEmpty();
    }

    #endregion

    #region Response record

    [Fact]
    public void Response_ShouldMapFromResultFields()
    {
        // Arrange
        var spot1 = new ArticlePromotionSpotDto(1, new List<ArticleSummaryDto>());
        var spot2 = new ArticlePromotionSpotDto(2, new List<ArticleSummaryDto>());
        var spot3 = new ArticlePromotionSpot3Dto(3, new List<ArticlePromotionSlotDto>());
        IReadOnlyList<ArticleSummaryDto> gossipStrip = new List<ArticleSummaryDto>();

        // Act
        var response = new PublicGetArticlePromotionFeedResponse(spot1, spot2, spot3, gossipStrip);

        // Assert
        response.Spot1.Should().Be(spot1);
        response.Spot2.Should().Be(spot2);
        response.Spot3.Should().Be(spot3);
        response.GossipStrip.Should().BeSameAs(gossipStrip);
    }

    #endregion

    #region StripSize query parameter

    [Fact]
    public async Task Handle_WhenStripSizeIsCustom_ShouldReturnThatManyItemsInGossipStrip()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());
        List<ArticleEntity> gossipPool = ArticleFactory.CreateManyPublished(CategoryId, 15);

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetGossipFallback(gossipPool);

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(StripSize: 5),
            CancellationToken.None
        );

        // Assert — pool consumed 4 fallbacks (spot1 + spot2 + spot3 col A + col B),
        // leaving 11; with stripSize 5, the strip should return 5
        result.GossipStrip.Should().HaveCount(5);
    }

    #endregion

    #region GossipStrip deduplication

    [Fact]
    public async Task Handle_WhenFallbacksUsed_GossipStripShouldNotContainFallbackArticles()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());
        List<ArticleEntity> gossipPool = ArticleFactory.CreateManyPublished(CategoryId, 10);

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, new List<ArticleEntity>());
        _articleRepositoryMock.SetupGetGossipFallback(gossipPool);

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(),
            CancellationToken.None
        );

        // Collect all fallback IDs (spot1, spot2, slot a, slot b)
        var fallbackIds = new HashSet<Guid>(
            result
                .Spot1.Articles.Select(a => a.Id)
                .Concat(result.Spot2.Articles.Select(a => a.Id))
                .Concat(result.Spot3.Slots.SelectMany(s => s.Articles.Select(a => a.Id)))
        );

        IEnumerable<Guid> stripIds = result.GossipStrip.Select(a => a.Id);

        // Assert — strip articles must not overlap with fallbacks
        stripIds.Should().NotContain(id => fallbackIds.Contains(id));
    }

    #endregion

    #region Interaction state flags

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldReturnAllFalseFlagsAndSkipBatchLookups()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());
        List<ArticleEntity> spot1 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> spot2 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> spot3 = ArticleFactory.CreateManyPublished(CategoryId, 2);
        List<ArticleEntity> gossipPool = ArticleFactory.CreateManyPublished(CategoryId, 5);

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, spot1);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, spot2);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, spot3);
        _articleRepositoryMock.SetupGetGossipFallback(gossipPool);

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        IEnumerable<ArticleSummaryDto> allSummaries = result
            .Spot1.Articles.Concat(result.Spot2.Articles)
            .Concat(result.Spot3.Slots.SelectMany(slot => slot.Articles))
            .Concat(result.GossipStrip);

        allSummaries.Should().OnlyContain(article => !article.IsLiked && !article.IsBookmarked);
        _articleRepositoryMock.VerifyGetLikedAndBookmarkedIdsCalledWithUser(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldStampAllSubCollections_WithSingleBatchPerType()
    {
        // Arrange
        CategoryEntity gossipCategory = CategoryFactory.Create(Guid.NewGuid());
        List<ArticleEntity> spot1 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> spot2 = ArticleFactory.CreateManyPublished(CategoryId, 1);
        List<ArticleEntity> spot3 = ArticleFactory.CreateManyPublished(CategoryId, 2);
        List<ArticleEntity> gossipPool = ArticleFactory.CreateManyPublished(CategoryId, 5);

        Guid likedSpot1Id = spot1[0].Id;
        Guid bookmarkedGossipId = gossipPool[0].Id;

        _categoryRepositoryMock.SetupGetGossipCategory(gossipCategory);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(1, spot1);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(2, spot2);
        _articleRepositoryMock.SetupGetActivePromotedBySpot(3, spot3);
        _articleRepositoryMock.SetupGetGossipFallback(gossipPool);
        _articleRepositoryMock.SetupGetLikedAndBookmarkedIds([likedSpot1Id], [bookmarkedGossipId]);

        // Act
        PublicGetArticlePromotionFeedResult result = await _handler.Handle(
            new PublicGetArticlePromotionFeedQuery(CurrentUserId: Guid.NewGuid()),
            CancellationToken.None
        );

        // Assert — the liked spot 1 article and the bookmarked gossip strip article are stamped
        result.Spot1.Articles.Single(article => article.Id == likedSpot1Id).IsLiked.Should().BeTrue();
        result.GossipStrip.Single(article => article.Id == bookmarkedGossipId).IsBookmarked.Should().BeTrue();
        result.Spot2.Articles.Should().OnlyContain(article => !article.IsLiked && !article.IsBookmarked);

        // The batch lookup runs exactly once across all sub-collections
        _articleRepositoryMock.VerifyGetLikedAndBookmarkedIdsCalledWithUser(Times.Once());
    }

    #endregion
}
