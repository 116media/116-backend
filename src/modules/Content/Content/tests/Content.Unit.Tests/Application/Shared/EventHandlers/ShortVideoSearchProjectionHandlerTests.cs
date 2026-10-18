using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.EventHandlers;

/// <summary>
/// Unit tests for <see cref="ShortVideoSearchProjectionHandler" />. A short has no editorial
/// workflow, so one change event carries every revision and the visibility flip alike; both
/// must reproject rather than remove.
/// </summary>
public class ShortVideoSearchProjectionHandlerTests
{
    private readonly Mock<ISearchProjection> _projectionMock = new();
    private readonly ShortVideoSearchProjectionHandler _handler;
    private readonly Guid _shortVideoId = Guid.NewGuid();

    public ShortVideoSearchProjectionHandlerTests()
    {
        _handler = new ShortVideoSearchProjectionHandler(_projectionMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAShortChanges_ShouldReprojectIt()
    {
        // Act
        await _handler.Handle(new ShortVideoChangedEvent(_shortVideoId));

        // Assert
        _projectionMock.Verify(
            p => p.ProjectAsync(EnumCoreContentType.Short, _shortVideoId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _projectionMock.Verify(
            p => p.RemoveAsync(It.IsAny<EnumCoreContentType>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "a deactivated short keeps its row, hidden"
        );
    }

    [Fact]
    public async Task Handle_WhenAShortIsDeleted_ShouldRemoveItsDocument()
    {
        // Act
        await _handler.Handle(new ShortVideoDeletedEvent(_shortVideoId, null, null));

        // Assert
        _projectionMock.Verify(
            p => p.RemoveAsync(EnumCoreContentType.Short, _shortVideoId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
