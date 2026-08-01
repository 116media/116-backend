using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Domain.Events;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.EventHandlers;

/// <summary>
/// Unit tests for <see cref="PopularTagsCacheHandler"/>.
/// </summary>
public class PopularTagsCacheHandlerTests
{
    private readonly Mock<IPopularTagsCacheInvalidator> _cacheInvalidatorMock;
    private readonly PopularTagsCacheHandler _handler;

    public PopularTagsCacheHandlerTests()
    {
        _cacheInvalidatorMock = MockPopularTagsCacheInvalidator.Create();
        _handler = new PopularTagsCacheHandler(_cacheInvalidatorMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTagGraphChanged_ShouldInvalidateOnce()
    {
        // Act
        await _handler.Handle(new TagGraphChangedEvent(TagId: Guid.NewGuid()), CancellationToken.None);

        // Assert
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }
}
