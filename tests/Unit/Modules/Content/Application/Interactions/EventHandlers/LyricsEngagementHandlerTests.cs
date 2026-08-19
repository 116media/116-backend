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
/// Unit tests for <see cref="LyricsEngagementHandler"/>.
/// </summary>
public class LyricsEngagementHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly LyricsEngagementHandler _handler;

    public LyricsEngagementHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new LyricsEngagementHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<LyricsEngagementHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WhenLikeAdded_ShouldIncrementLikeCountAndCommit()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdAsync(lyrics.Id, lyrics);

        // Act
        await _handler.Handle(new LyricsEngagedEvent(lyrics.Id, EnumEngagementKind.Like, 1), CancellationToken.None);

        // Assert
        lyrics.LikeCount.Should().Be(1);
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLikeRemoved_ShouldDecrementLikeCountAndCommit()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        lyrics.IncrementLikeCount();
        _lyricsRepositoryMock.SetupGetByIdAsync(lyrics.Id, lyrics);

        // Act
        await _handler.Handle(new LyricsEngagedEvent(lyrics.Id, EnumEngagementKind.Like, -1), CancellationToken.None);

        // Assert
        lyrics.LikeCount.Should().Be(0);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenShared_ShouldIncrementShareCountAndCommit()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdAsync(lyrics.Id, lyrics);

        // Act
        await _handler.Handle(new LyricsEngagedEvent(lyrics.Id, EnumEngagementKind.Share, 1), CancellationToken.None);

        // Assert
        lyrics.ShareCount.Should().Be(1);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenCountedView_ShouldIncrementViewCountAndCommit()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdAsync(lyrics.Id, lyrics);

        // Act
        await _handler.Handle(new LyricsEngagedEvent(lyrics.Id, EnumEngagementKind.View, 1), CancellationToken.None);

        // Assert
        lyrics.ViewCount.Should().Be(1);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithKindWithoutLyricsCounter_ShouldNotCommit()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdAsync(lyrics.Id, lyrics);

        // Act
        await _handler.Handle(
            new LyricsEngagedEvent(lyrics.Id, EnumEngagementKind.Bookmark, 1),
            CancellationToken.None
        );

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsMissing_ShouldSkipWithoutCommit()
    {
        // Arrange — the lyrics page vanished between the interaction commit
        // and the dispatch, which is a race, not an error.
        Guid lyricsId = Guid.NewGuid();
        _lyricsRepositoryMock.SetupGetByIdAsync(lyricsId, null);

        // Act
        await _handler.Handle(new LyricsEngagedEvent(lyricsId, EnumEngagementKind.Like, 1), CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.Verify(x => x.Update(It.IsAny<LyricsEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
