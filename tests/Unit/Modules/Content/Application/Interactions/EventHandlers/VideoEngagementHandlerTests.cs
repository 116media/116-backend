using _116.Content.Application.Interactions.EventHandlers;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Unit tests for <see cref="VideoEngagementHandler"/>.
/// </summary>
public class VideoEngagementHandlerTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPopularVideosCacheInvalidator> _cacheInvalidatorMock;
    private readonly VideoEngagementHandler _handler;

    public VideoEngagementHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _cacheInvalidatorMock = MockPopularVideosCacheInvalidator.Create();
        _handler = new VideoEngagementHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheInvalidatorMock.Object,
            NullLogger<VideoEngagementHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WhenShare_ShouldIncrementShareCountCommitAndInvalidate()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        _videoRepositoryMock.SetupGetByIdAsync(video.Id, video);

        // Act
        await _handler.Handle(new VideoEngagedEvent(video.Id, EnumEngagementKind.Share, 1), CancellationToken.None);

        // Assert
        video.ShareCount.Should().Be(1);
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }

    [Fact]
    public async Task Handle_WhenRating_ShouldRecomputeAggregatesFromRowsCommitAndInvalidate()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        _videoRepositoryMock.SetupGetByIdAsync(video.Id, video);
        _videoRepositoryMock.SetupGetAllRatingsForVideoAsync([
            VideoRatingFactory.Create(video.Id, Guid.NewGuid(), stars: 5),
            VideoRatingFactory.Create(video.Id, Guid.NewGuid(), stars: 4),
        ]);

        // Act
        await _handler.Handle(new VideoEngagedEvent(video.Id, EnumEngagementKind.Rating, 1), CancellationToken.None);

        // Assert
        video.RatingCount.Should().Be(2);
        video.RatingAverage.Should().Be(4.5m);
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }

    [Fact]
    public async Task Handle_WithKindWithoutVideoCounter_ShouldOnlyInvalidate()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        _videoRepositoryMock.SetupGetByIdAsync(video.Id, video);

        // Act
        await _handler.Handle(new VideoEngagedEvent(video.Id, EnumEngagementKind.Like, 1), CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoMissing_ShouldSkipWithoutCommitOrInvalidation()
    {
        // Arrange — the video vanished between the interaction commit and the
        // dispatch, which is a race, not an error.
        Guid videoId = Guid.NewGuid();
        _videoRepositoryMock.SetupGetByIdAsync(videoId, null);

        // Act
        await _handler.Handle(new VideoEngagedEvent(videoId, EnumEngagementKind.Share, 1), CancellationToken.None);

        // Assert
        _videoRepositoryMock.Verify(x => x.Update(It.IsAny<VideoEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitNotCalled();
        _cacheInvalidatorMock.VerifyInvalidateNotCalled();
    }
}
