using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Domain.Events;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.EventHandlers;

/// <summary>
/// Unit tests for <see cref="PopularVideosCacheHandler"/>.
/// </summary>
public class PopularVideosCacheHandlerTests
{
    private readonly Mock<IPopularVideosCacheInvalidator> _cacheInvalidatorMock;
    private readonly PopularVideosCacheHandler _handler;

    public PopularVideosCacheHandlerTests()
    {
        _cacheInvalidatorMock = MockPopularVideosCacheInvalidator.Create();
        _handler = new PopularVideosCacheHandler(_cacheInvalidatorMock.Object);
    }

    [Fact]
    public async Task Handle_WhenVideoPublished_ShouldInvalidateOnce()
    {
        // Act
        await _handler.Handle(new VideoPublishedEvent(VideoId: Guid.NewGuid()), CancellationToken.None);

        // Assert
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoUnpublished_ShouldInvalidateOnce()
    {
        // Act
        await _handler.Handle(new VideoUnpublishedEvent(VideoId: Guid.NewGuid()), CancellationToken.None);

        // Assert
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoDeleted_ShouldInvalidateOnce()
    {
        // Act
        await _handler.Handle(
            new VideoDeletedEvent(VideoId: Guid.NewGuid(), ThumbnailFileId: null),
            CancellationToken.None
        );

        // Assert
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }
}
