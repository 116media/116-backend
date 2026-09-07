using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Domain.Events;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.EventHandlers;

/// <summary>
/// Unit tests for <see cref="PopularArticlesCacheHandler"/>.
/// </summary>
public class PopularArticlesCacheHandlerTests
{
    private readonly Mock<HybridCache> _cacheMock;
    private readonly PopularArticlesCacheHandler _handler;

    public PopularArticlesCacheHandlerTests()
    {
        _cacheMock = MockHybridCache.Create();
        _handler = new PopularArticlesCacheHandler(_cacheMock.Object);
    }

    [Fact]
    public async Task Handle_WhenArticlePublished_ShouldInvalidateOnce()
    {
        // Act
        await _handler.Handle(new ArticlePublishedEvent(ArticleId: Guid.NewGuid()), CancellationToken.None);

        // Assert
        _cacheMock.VerifyRemovedByTag(ContentCacheTags.PopularArticles);
    }

    [Fact]
    public async Task Handle_WhenArticleUnpublished_ShouldInvalidateOnce()
    {
        // Act
        await _handler.Handle(new ArticleUnpublishedEvent(ArticleId: Guid.NewGuid()), CancellationToken.None);

        // Assert
        _cacheMock.VerifyRemovedByTag(ContentCacheTags.PopularArticles);
    }

    [Fact]
    public async Task Handle_WhenArticleDeleted_ShouldInvalidateOnce()
    {
        // Act
        await _handler.Handle(
            new ArticleDeletedEvent(ArticleId: Guid.NewGuid(), CoverFileId: null, BodyImageStorageKeys: []),
            CancellationToken.None
        );

        // Assert
        _cacheMock.VerifyRemovedByTag(ContentCacheTags.PopularArticles);
    }
}
