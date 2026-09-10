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
/// Unit tests for <see cref="LyricsEngagementHandler"/>. The counter itself is applied in SQL, so these assert
/// the delta the handler forwards; the arithmetic is proven against the database in the
/// repository and workflow integration tests.
/// </summary>
public class LyricsEngagementHandlerTests
{
    private readonly Mock<ILyricsRepository> _repositoryMock;
    private readonly LyricsEngagementHandler _handler;

    public LyricsEngagementHandlerTests()
    {
        _repositoryMock = MockLyricsRepository.Create();
        _handler = new LyricsEngagementHandler(_repositoryMock.Object, NullLogger<LyricsEngagementHandler>.Instance);
    }

    [Theory]
    [InlineData(EnumEngagementKind.Like, 1)]
    [InlineData(EnumEngagementKind.Like, -1)]
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
        await _handler.Handle(new LyricsEngagedEvent(id, kind, delta), CancellationToken.None);

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
        // Arrange
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
            await _handler.Handle(new LyricsEngagedEvent(id, EnumEngagementKind.Like, 1), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
