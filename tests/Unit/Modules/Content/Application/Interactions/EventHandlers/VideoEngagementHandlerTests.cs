using _116.Content.Application.Interactions.EventHandlers;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Unit tests for <see cref="VideoEngagementHandler"/>. Shares forward a delta applied in SQL;
/// ratings are recomputed from the committed rating rows and written outright, so the event's
/// delta is never trusted for them.
/// </summary>
public class VideoEngagementHandlerTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<HybridCache> _cacheMock;
    private readonly VideoEngagementHandler _handler;

    public VideoEngagementHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _cacheMock = MockHybridCache.Create();
        _handler = new VideoEngagementHandler(
            _videoRepositoryMock.Object,
            _cacheMock.Object,
            NullLogger<VideoEngagementHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WhenShare_ShouldForwardTheDeltaAndInvalidate()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        _videoRepositoryMock
            .Setup(x =>
                x.ApplyEngagementDeltaAsync(videoId, EnumEngagementKind.Share, 1, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new VideoEngagedEvent(videoId, EnumEngagementKind.Share, 1), CancellationToken.None);

        // Assert — loading to mutate is the race stage 8 removed; the counter moves in SQL only.
        _videoRepositoryMock.Verify(
            x => x.ApplyEngagementDeltaAsync(videoId, EnumEngagementKind.Share, 1, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _videoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.VerifyRemovedByTag(ContentCacheTags.PopularVideos);
    }

    [Fact]
    public async Task Handle_WhenRated_ShouldWriteTheRecomputedAverageNotTheDelta()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        List<VideoRatingEntity> ratings =
        [
            VideoRatingFactory.Create(video.Id, Guid.NewGuid(), stars: 3),
            VideoRatingFactory.Create(video.Id, Guid.NewGuid(), stars: 4),
            VideoRatingFactory.Create(video.Id, Guid.NewGuid(), stars: 5),
        ];
        _videoRepositoryMock.SetupGetAllRatingsForVideoAsync(ratings);
        _videoRepositoryMock
            .Setup(x => x.SetRatingAsync(video.Id, It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new VideoEngagedEvent(video.Id, EnumEngagementKind.Rating, 99), CancellationToken.None);

        // Assert
        _videoRepositoryMock.Verify(
            x => x.SetRatingAsync(video.Id, 4.00m, 3, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _videoRepositoryMock.Verify(
            x =>
                x.ApplyEngagementDeltaAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<EnumEngagementKind>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _cacheMock.VerifyRemovedByTag(ContentCacheTags.PopularVideos);
    }

    [Fact]
    public async Task Handle_WhenRatedWithNoRatingsLeft_ShouldWriteZero()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        _videoRepositoryMock.SetupGetAllRatingsForVideoAsync([]);
        _videoRepositoryMock
            .Setup(x => x.SetRatingAsync(videoId, It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new VideoEngagedEvent(videoId, EnumEngagementKind.Rating, -1), CancellationToken.None);

        // Assert
        _videoRepositoryMock.Verify(x => x.SetRatingAsync(videoId, 0m, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoRowIsUpdated_ShouldStillInvalidate()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        _videoRepositoryMock
            .Setup(x =>
                x.ApplyEngagementDeltaAsync(videoId, EnumEngagementKind.Share, 1, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(0);

        // Act
        await _handler.Handle(new VideoEngagedEvent(videoId, EnumEngagementKind.Share, 1), CancellationToken.None);

        // Assert
        _cacheMock.VerifyRemovedByTag(ContentCacheTags.PopularVideos);
    }
}
