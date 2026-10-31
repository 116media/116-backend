using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.EventHandlers;

/// <summary>
/// Unit tests for <see cref="VideoSearchProjectionHandler" />. Each transition must reach the
/// projection with the video's own content type, or the document is written against the wrong
/// key and search answers with another type's row.
/// </summary>
public class VideoSearchProjectionHandlerTests
{
    private readonly Mock<ISearchProjection> _projectionMock = new();
    private readonly VideoSearchProjectionHandler _handler;
    private readonly Guid _videoId = Guid.NewGuid();

    public VideoSearchProjectionHandlerTests()
    {
        _handler = new VideoSearchProjectionHandler(_projectionMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAVideoIsPublished_ShouldProjectIt()
    {
        // Act
        await _handler.Handle(new VideoPublishedEvent(_videoId));

        // Assert
        _projectionMock.Verify(
            p => p.ProjectAsync(EnumCoreContentType.Video, _videoId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenAVideoIsUnpublished_ShouldReprojectRatherThanRemove()
    {
        // Act
        await _handler.Handle(new VideoUnpublishedEvent(_videoId));

        // Assert
        _projectionMock.Verify(
            p => p.ProjectAsync(EnumCoreContentType.Video, _videoId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _projectionMock.Verify(
            p => p.RemoveAsync(It.IsAny<EnumCoreContentType>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "an unpublished video keeps its row, hidden"
        );
    }

    [Fact]
    public async Task Handle_WhenAVideoIsDeleted_ShouldRemoveItsDocument()
    {
        // Act
        await _handler.Handle(new VideoDeletedEvent(_videoId, null));

        // Assert
        _projectionMock.Verify(
            p => p.RemoveAsync(EnumCoreContentType.Video, _videoId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
