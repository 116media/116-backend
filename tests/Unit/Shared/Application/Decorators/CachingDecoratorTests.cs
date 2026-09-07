using _116.Shared.Application.Decorators;
using _116.Shared.Contracts.Application.CQRS;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Decorators;

/// <summary>
/// Unit tests for <see cref="CachingDecorator{TRequest,TResponse}"/>, run against a real
/// <see cref="HybridCache"/> built from a service collection so hit, miss, conditional bypass,
/// tag eviction and stampede coordination are exercised without mocking the cache.
/// </summary>
public class CachingDecoratorTests
{
    #region Test Setup

    private const string Tag = "test:responses";

    public record PlainRequest(string Value) : IRequest<TestResponse>;

    public record CacheableRequest(string Value) : IRequest<TestResponse>, ICacheableRequest
    {
        public string CacheKey => $"cacheable:{Value}";

        public TimeSpan Ttl => TimeSpan.FromMinutes(5);

        public IReadOnlyList<string> CacheTags => [Tag];
    }

    public record ConditionalRequest(string Value, bool Cacheable)
        : IRequest<TestResponse>,
            IConditionallyCacheableRequest
    {
        public bool IsCacheable => Cacheable;

        public string CacheKey => $"conditional:{Value}";

        public TimeSpan Ttl => TimeSpan.FromMinutes(5);

        public IReadOnlyList<string> CacheTags => [Tag];
    }

    public record TestResponse(string Result);

    private static HybridCache BuildCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    private static Mock<IRequestHandler<TRequest, TestResponse>> BuildHandler<TRequest>()
        where TRequest : IRequest<TestResponse>
    {
        Mock<IRequestHandler<TRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("fresh"));
        return handlerMock;
    }

    #endregion

    #region Pass-through

    [Fact]
    public async Task Handle_WithNonCacheableRequest_ShouldReachTheHandlerEveryTime()
    {
        // Arrange
        Mock<IRequestHandler<PlainRequest, TestResponse>> handlerMock = BuildHandler<PlainRequest>();
        CachingDecorator<PlainRequest, TestResponse> decorator = new(handlerMock.Object, BuildCache());

        var request = new PlainRequest("a");

        // Act
        await decorator.Handle(request, CancellationToken.None);
        await decorator.Handle(request, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenIsCacheableIsFalse_ShouldReachTheHandlerEveryTime()
    {
        // Arrange
        Mock<IRequestHandler<ConditionalRequest, TestResponse>> handlerMock = BuildHandler<ConditionalRequest>();
        CachingDecorator<ConditionalRequest, TestResponse> decorator = new(handlerMock.Object, BuildCache());

        var request = new ConditionalRequest("a", Cacheable: false);

        // Act
        await decorator.Handle(request, CancellationToken.None);
        await decorator.Handle(request, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion

    #region Caching

    [Fact]
    public async Task Handle_WithCacheableRequest_ShouldReachTheHandlerOnceAcrossTwoCalls()
    {
        // Arrange
        Mock<IRequestHandler<CacheableRequest, TestResponse>> handlerMock = BuildHandler<CacheableRequest>();
        CachingDecorator<CacheableRequest, TestResponse> decorator = new(handlerMock.Object, BuildCache());

        var request = new CacheableRequest("a");

        // Act
        TestResponse first = await decorator.Handle(request, CancellationToken.None);
        TestResponse second = await decorator.Handle(request, CancellationToken.None);

        // Assert
        first.Result.Should().Be("fresh");
        second.Result.Should().Be("fresh");
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDistinctCacheKeys_ShouldReachTheHandlerForEach()
    {
        // Arrange
        Mock<IRequestHandler<CacheableRequest, TestResponse>> handlerMock = BuildHandler<CacheableRequest>();
        CachingDecorator<CacheableRequest, TestResponse> decorator = new(handlerMock.Object, BuildCache());

        // Act
        await decorator.Handle(new CacheableRequest("a"), CancellationToken.None);
        await decorator.Handle(new CacheableRequest("b"), CancellationToken.None);

        // Assert
        handlerMock.Verify(
            h => h.Handle(It.IsAny<CacheableRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_WhenIsCacheableIsTrue_ShouldCacheTheConditionalRequest()
    {
        // Arrange
        Mock<IRequestHandler<ConditionalRequest, TestResponse>> handlerMock = BuildHandler<ConditionalRequest>();
        CachingDecorator<ConditionalRequest, TestResponse> decorator = new(handlerMock.Object, BuildCache());

        var request = new ConditionalRequest("a", Cacheable: true);

        // Act
        await decorator.Handle(request, CancellationToken.None);
        await decorator.Handle(request, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Eviction

    [Fact]
    public async Task Handle_AfterTagEviction_ShouldReachTheHandlerAgain()
    {
        // Arrange
        Mock<IRequestHandler<CacheableRequest, TestResponse>> handlerMock = BuildHandler<CacheableRequest>();
        HybridCache cache = BuildCache();
        CachingDecorator<CacheableRequest, TestResponse> decorator = new(handlerMock.Object, cache);

        var request = new CacheableRequest("a");

        // Act — cache the result, evict its tag, read again
        await decorator.Handle(request, CancellationToken.None);
        await cache.RemoveByTagAsync(Tag, CancellationToken.None);
        await decorator.Handle(request, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion

    #region Stampede protection

    [Fact]
    public async Task Handle_WithConcurrentMissesOnOneKey_ShouldRunTheHandlerOnce()
    {
        // Arrange — a slow handler so every concurrent caller arrives during the first fill
        int calls = 0;
        Mock<IRequestHandler<CacheableRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<CacheableRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                Interlocked.Increment(ref calls);
                await Task.Delay(100);
                return new TestResponse("fresh");
            });

        CachingDecorator<CacheableRequest, TestResponse> decorator = new(handlerMock.Object, BuildCache());
        var request = new CacheableRequest("hot");

        // Act — ten concurrent requests for the same cold key
        TestResponse[] results = await Task.WhenAll(
            Enumerable.Range(0, 10).Select(_ => decorator.Handle(request, CancellationToken.None))
        );

        // Assert — the factory ran once and every caller got the same result
        calls.Should().Be(1);
        results.Should().AllSatisfy(r => r.Result.Should().Be("fresh"));
    }

    #endregion
}
