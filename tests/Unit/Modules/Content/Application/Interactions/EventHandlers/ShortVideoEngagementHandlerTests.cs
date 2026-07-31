using _116.Content.Application.Interactions.EventHandlers;
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
/// Unit tests for <see cref="ShortVideoEngagementHandler"/>.
/// </summary>
public class ShortVideoEngagementHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ShortVideoEngagementHandler _handler;

    public ShortVideoEngagementHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new ShortVideoEngagementHandler(
            _shortVideoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<ShortVideoEngagementHandler>.Instance
        );
    }

    [Theory]
    [InlineData(EnumEngagementKind.Like)]
    [InlineData(EnumEngagementKind.Bookmark)]
    [InlineData(EnumEngagementKind.Share)]
    [InlineData(EnumEngagementKind.View)]
    public async Task Handle_WithPositiveDelta_ShouldIncrementMatchingCounterAndCommit(EnumEngagementKind kind)
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _shortVideoRepositoryMock.SetupGetByIdAsync(shortVideo.Id, shortVideo);

        // Act
        await _handler.Handle(new ShortVideoEngagedEvent(shortVideo.Id, kind, 1), CancellationToken.None);

        // Assert
        int total = shortVideo.LikeCount + shortVideo.BookmarkCount + shortVideo.ShareCount + shortVideo.ViewCount;
        total.Should().Be(1);
        _shortVideoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Theory]
    [InlineData(EnumEngagementKind.Like)]
    [InlineData(EnumEngagementKind.Bookmark)]
    public async Task Handle_WithNegativeDelta_ShouldDecrementMatchingCounterAndCommit(EnumEngagementKind kind)
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        shortVideo.IncrementLikeCount();
        shortVideo.IncrementBookmarkCount();
        _shortVideoRepositoryMock.SetupGetByIdAsync(shortVideo.Id, shortVideo);

        // Act
        await _handler.Handle(new ShortVideoEngagedEvent(shortVideo.Id, kind, -1), CancellationToken.None);

        // Assert
        int total = shortVideo.LikeCount + shortVideo.BookmarkCount;
        total.Should().Be(1);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithKindWithoutShortVideoCounter_ShouldNotCommit()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _shortVideoRepositoryMock.SetupGetByIdAsync(shortVideo.Id, shortVideo);

        // Act
        await _handler.Handle(
            new ShortVideoEngagedEvent(shortVideo.Id, EnumEngagementKind.Comment, 1),
            CancellationToken.None
        );

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoMissing_ShouldSkipWithoutCommit()
    {
        // Arrange — the short video vanished between the interaction commit
        // and the dispatch, which is a race, not an error.
        Guid shortVideoId = Guid.NewGuid();
        _shortVideoRepositoryMock.SetupGetByIdAsync(shortVideoId, null);

        // Act
        await _handler.Handle(
            new ShortVideoEngagedEvent(shortVideoId, EnumEngagementKind.Like, 1),
            CancellationToken.None
        );

        // Assert
        _shortVideoRepositoryMock.Verify(x => x.Update(It.IsAny<ShortVideoEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
