using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Domain.Events;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.EventHandlers;

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
