using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed;

/// <summary>
/// Unit tests for <see cref="PublicGetVideoPromotionFeedHandler"/>.
/// </summary>
public class PublicGetVideoPromotionFeedHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetVideoPromotionFeedHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetVideoPromotionFeedHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new PublicGetVideoPromotionFeedHandler(
            _videoRepositoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    #region All spots filled

    [Fact]
    public async Task Handle_WhenAllSpotsHavePromotedVideos_ShouldReturnPromotedVideosWithNoFallbacks()
    {
        // Arrange
        List<VideoEntity> spot1 = VideoFactory.CreateManyPublished(CategoryId, 1);
        List<VideoEntity> spot2 = VideoFactory.CreateManyPublished(CategoryId, 1);
        List<VideoEntity> spot3 = VideoFactory.CreateManyPublished(CategoryId, 2);
        List<VideoEntity> freePool = VideoFactory.CreateManyPublished(CategoryId, 5);

        _videoRepositoryMock.SetupGetActivePromotedBySpot(1, spot1);
        _videoRepositoryMock.SetupGetActivePromotedBySpot(2, spot2);
        _videoRepositoryMock.SetupGetActivePromotedBySpot(3, spot3);
        _videoRepositoryMock.SetupGetFreeVideos(freePool);

        // Act
        PublicGetVideoPromotionFeedResult result = await _handler.Handle(
            new PublicGetVideoPromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot1.Videos.Should().ContainSingle();
        result.Spot2.Videos.Should().ContainSingle();
        result.Spot3.Slots.Should().HaveCount(2);
        result.FreeVideoStrip.Should().HaveCount(3);
    }

    #endregion

    #region Spot 1 empty — fallback

    [Fact]
    public async Task Handle_WhenSpot1Empty_ShouldFallBackToOneFreeVideo()
    {
        // Arrange
        List<VideoEntity> spot2 = VideoFactory.CreateManyPublished(CategoryId, 1);
        List<VideoEntity> spot3 = VideoFactory.CreateManyPublished(CategoryId, 1);
        List<VideoEntity> freePool = VideoFactory.CreateManyPublished(CategoryId, 5);

        _videoRepositoryMock.SetupGetActivePromotedBySpot(1, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(2, spot2);
        _videoRepositoryMock.SetupGetActivePromotedBySpot(3, spot3);
        _videoRepositoryMock.SetupGetFreeVideos(freePool);

        // Act
        PublicGetVideoPromotionFeedResult result = await _handler.Handle(
            new PublicGetVideoPromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot1.SpotPriority.Should().Be(1);
        result.Spot1.Videos.Should().ContainSingle();
        result.Spot2.Videos.Should().ContainSingle();
    }

    #endregion

    #region Spot 2 empty — fallback

    [Fact]
    public async Task Handle_WhenSpot2Empty_ShouldFallBackToOneFreeVideo()
    {
        // Arrange
        List<VideoEntity> spot1 = VideoFactory.CreateManyPublished(CategoryId, 1);
        List<VideoEntity> spot3 = VideoFactory.CreateManyPublished(CategoryId, 1);
        List<VideoEntity> freePool = VideoFactory.CreateManyPublished(CategoryId, 5);

        _videoRepositoryMock.SetupGetActivePromotedBySpot(1, spot1);
        _videoRepositoryMock.SetupGetActivePromotedBySpot(2, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(3, spot3);
        _videoRepositoryMock.SetupGetFreeVideos(freePool);

        // Act
        PublicGetVideoPromotionFeedResult result = await _handler.Handle(
            new PublicGetVideoPromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot2.SpotPriority.Should().Be(2);
        result.Spot2.Videos.Should().ContainSingle();
    }

    #endregion

    #region Spot 3 partial — one promoted, one fallback

    [Fact]
    public async Task Handle_WhenSpot3HasOnePromo_ShouldPutItInColumnAAndFallbackInColumnB()
    {
        // Arrange
        List<VideoEntity> spot3 = VideoFactory.CreateManyPublished(CategoryId, 1);
        List<VideoEntity> freePool = VideoFactory.CreateManyPublished(CategoryId, 5);

        _videoRepositoryMock.SetupGetActivePromotedBySpot(1, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(2, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(3, spot3);
        _videoRepositoryMock.SetupGetFreeVideos(freePool);

        // Act
        PublicGetVideoPromotionFeedResult result = await _handler.Handle(
            new PublicGetVideoPromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        VideoPromotionSlotDto slotA = result.Spot3.Slots.Single(s => s.Position == "a");
        VideoPromotionSlotDto slotB = result.Spot3.Slots.Single(s => s.Position == "b");
        slotA.Videos.Should().ContainSingle();
        slotB.Videos.Should().ContainSingle();
    }

    #endregion

    #region Spot 3 fully empty — both columns get fallbacks

    [Fact]
    public async Task Handle_WhenSpot3FullyEmpty_ShouldFillBothColumnsWithFreeVideos()
    {
        // Arrange
        List<VideoEntity> freePool = VideoFactory.CreateManyPublished(CategoryId, 10);

        _videoRepositoryMock.SetupGetActivePromotedBySpot(1, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(2, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(3, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetFreeVideos(freePool);

        // Act
        PublicGetVideoPromotionFeedResult result = await _handler.Handle(
            new PublicGetVideoPromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        VideoPromotionSlotDto slotA = result.Spot3.Slots.Single(s => s.Position == "a");
        VideoPromotionSlotDto slotB = result.Spot3.Slots.Single(s => s.Position == "b");
        slotA.Videos.Should().ContainSingle();
        slotB.Videos.Should().ContainSingle();
    }

    #endregion

    #region All spots empty

    [Fact]
    public async Task Handle_WhenAllSpotsEmpty_ShouldFillAllFromFreePool()
    {
        // Arrange
        List<VideoEntity> freePool = VideoFactory.CreateManyPublished(CategoryId, 10);

        _videoRepositoryMock.SetupGetActivePromotedBySpot(1, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(2, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(3, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetFreeVideos(freePool);

        // Act
        PublicGetVideoPromotionFeedResult result = await _handler.Handle(
            new PublicGetVideoPromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot1.Videos.Should().ContainSingle();
        result.Spot2.Videos.Should().ContainSingle();
        result.Spot3.SpotPriority.Should().Be(3);
        result.FreeVideoStrip.Count.Should().BeLessThanOrEqualTo(3);
    }

    #endregion

    #region Free pool exhausted

    [Fact]
    public async Task Handle_WhenFreePoolEmpty_ShouldReturnEmptySpotsWithNoFallback()
    {
        // Arrange
        _videoRepositoryMock.SetupGetActivePromotedBySpot(1, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(2, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(3, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetFreeVideos(new List<VideoEntity>());

        // Act
        PublicGetVideoPromotionFeedResult result = await _handler.Handle(
            new PublicGetVideoPromotionFeedQuery(),
            CancellationToken.None
        );

        // Assert
        result.Spot1.Videos.Should().BeEmpty();
        result.Spot2.Videos.Should().BeEmpty();
        result.FreeVideoStrip.Should().BeEmpty();
    }

    #endregion

    #region FreeVideoStrip deduplication

    [Fact]
    public async Task Handle_WhenFallbacksUsed_FreeVideoStripShouldNotContainFallbackVideos()
    {
        // Arrange
        List<VideoEntity> freePool = VideoFactory.CreateManyPublished(CategoryId, 10);

        _videoRepositoryMock.SetupGetActivePromotedBySpot(1, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(2, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(3, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetFreeVideos(freePool);

        // Act
        PublicGetVideoPromotionFeedResult result = await _handler.Handle(
            new PublicGetVideoPromotionFeedQuery(),
            CancellationToken.None
        );

        // Collect all fallback IDs (spot1, spot2, slot a, slot b)
        var fallbackIds = new HashSet<Guid>(
            result
                .Spot1.Videos.Select(v => v.Id)
                .Concat(result.Spot2.Videos.Select(v => v.Id))
                .Concat(result.Spot3.Slots.SelectMany(s => s.Videos.Select(v => v.Id)))
        );

        IEnumerable<Guid> stripIds = result.FreeVideoStrip.Select(v => v.Id);

        // Assert — strip videos must not overlap with fallbacks
        stripIds.Should().NotContain(id => fallbackIds.Contains(id));
    }

    #endregion

    #region Response record

    [Fact]
    public void Response_ShouldMapFromResultFields()
    {
        // Arrange
        var spot1 = new VideoPromotionSpotDto(1, new List<VideoSummaryDto>());
        var spot2 = new VideoPromotionSpotDto(2, new List<VideoSummaryDto>());
        var spot3 = new VideoPromotionSpot3Dto(3, new List<VideoPromotionSlotDto>());
        IReadOnlyList<VideoSummaryDto> freeVideoStrip = new List<VideoSummaryDto>();

        // Act
        var response = new PublicGetVideoPromotionFeedResponse(spot1, spot2, spot3, freeVideoStrip);

        // Assert
        response.Spot1.Should().Be(spot1);
        response.Spot2.Should().Be(spot2);
        response.Spot3.Should().Be(spot3);
        response.FreeVideoStrip.Should().BeSameAs(freeVideoStrip);
    }

    #endregion

    #region StripSize query parameter

    [Fact]
    public async Task Handle_WhenStripSizeIsCustom_ShouldReturnThatManyItemsInStrip()
    {
        // Arrange
        List<VideoEntity> freePool = VideoFactory.CreateMany(CategoryId, 10);

        _videoRepositoryMock.SetupGetActivePromotedBySpot(1, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(2, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetActivePromotedBySpot(3, new List<VideoEntity>());
        _videoRepositoryMock.SetupGetFreeVideos(freePool);

        // Act
        PublicGetVideoPromotionFeedResult result = await _handler.Handle(
            new PublicGetVideoPromotionFeedQuery(StripSize: 5),
            CancellationToken.None
        );

        // Assert — strip consumed 3 fallbacks (spot1 + spot2 + spot3 col B), leaving 7;
        // with stripSize 5, the strip should return 5
        result.FreeVideoStrip.Should().HaveCount(5);
    }

    #endregion
}
