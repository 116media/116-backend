using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Domain.Events;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.EventHandlers;

/// <summary>
/// Unit tests for <see cref="PopularTagsCacheHandler"/>.
/// </summary>
public class PopularTagsCacheHandlerTests
{
    private readonly Mock<HybridCache> _cacheMock;
    private readonly PopularTagsCacheHandler _handler;

    public PopularTagsCacheHandlerTests()
    {
        _cacheMock = MockHybridCache.Create();
        _handler = new PopularTagsCacheHandler(_cacheMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTagGraphChanged_ShouldInvalidateOnce()
    {
        // Act
        await _handler.Handle(new TagGraphChangedEvent(TagId: Guid.NewGuid()), CancellationToken.None);

        // Assert
        _cacheMock.VerifyRemovedByTag(ContentCacheTags.Tags);
    }
}
