using _116.Content.Application.Interactions.EventHandlers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Unit tests for <see cref="ShortVideoEngagementHandler"/>. The counter itself is applied in SQL, so these assert
/// the delta the handler forwards; the arithmetic is proven against the database in the
/// repository and workflow integration tests.
/// </summary>
public class ShortVideoEngagementHandlerTests
{
    private readonly Mock<IShortVideoRepository> _repositoryMock;
    private readonly ShortVideoEngagementHandler _handler;

    public ShortVideoEngagementHandlerTests()
    {
        _repositoryMock = MockShortVideoRepository.Create();
        _handler = new ShortVideoEngagementHandler(
            _repositoryMock.Object,
            NullLogger<ShortVideoEngagementHandler>.Instance
        );
    }

    [Theory]
    [InlineData(EnumEngagementKind.Like, 1)]
    [InlineData(EnumEngagementKind.Like, -1)]
    [InlineData(EnumEngagementKind.Bookmark, 1)]
    [InlineData(EnumEngagementKind.Bookmark, -1)]
    [InlineData(EnumEngagementKind.Share, 1)]
    [InlineData(EnumEngagementKind.View, 1)]
    public async Task Handle_ShouldForwardTheKindAndDeltaThenSkipTracking(EnumEngagementKind kind, int delta)
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.ApplyEngagementDeltaAsync(id, kind, delta, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new ShortVideoEngagedEvent(id, kind, delta), CancellationToken.None);

        // Assert — loading to mutate is the race stage 8 removed; the counter moves in SQL only.
        _repositoryMock.Verify(
            x => x.ApplyEngagementDeltaAsync(id, kind, delta, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNoRowIsUpdated_ShouldNotThrow()
    {
        // Arrange — the row vanished between the interaction commit and the dispatch, which is a
        // race, not an error.
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(x =>
                x.ApplyEngagementDeltaAsync(
                    id,
                    It.IsAny<EnumEngagementKind>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(0);

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(new ShortVideoEngagedEvent(id, EnumEngagementKind.Like, 1), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
