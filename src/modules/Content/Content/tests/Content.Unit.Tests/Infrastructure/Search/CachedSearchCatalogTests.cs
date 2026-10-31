using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Search;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Infrastructure.Search;

/// <summary>
/// Unit tests for <see cref="CachedSearchCatalog" />. The decorator must cache the counts
/// under every registered type's tag and leave the ranked pages uncached, since a page the
/// endpoint answers <c>no-store</c> must not be served from a shared cache.
/// </summary>
public class CachedSearchCatalogTests
{
    /// <summary>
    /// Counts for a query no provider answered.
    /// </summary>
    private static SearchCountsDto NoCounts => new(new Dictionary<EnumSearchScope, int>());

    private readonly Mock<ISearchCatalog> _innerMock = new();
    private readonly Mock<HybridCache> _cacheMock = new();
    private readonly CachedSearchCatalog _catalog;

    private string? _capturedKey;
    private string[]? _capturedTags;

    public CachedSearchCatalogTests()
    {
        _cacheMock
            .Setup(cache =>
                cache.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<(ISearchCatalog, string)>(),
                    It.IsAny<Func<(ISearchCatalog, string), CancellationToken, ValueTask<SearchCountsDto>>>(),
                    It.IsAny<HybridCacheEntryOptions?>(),
                    It.IsAny<IEnumerable<string>?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(
                (
                    string key,
                    (ISearchCatalog Inner, string Query) state,
                    Func<(ISearchCatalog, string), CancellationToken, ValueTask<SearchCountsDto>> factory,
                    HybridCacheEntryOptions? _,
                    IEnumerable<string>? tags,
                    CancellationToken token
                ) =>
                {
                    _capturedKey = key;
                    _capturedTags = tags?.ToArray();
                    return factory(state, token);
                }
            );

        _catalog = new CachedSearchCatalog(
            _innerMock.Object,
            _cacheMock.Object,
            [Projector(EnumCoreContentType.Article), Projector(EnumCoreContentType.Video)]
        );
    }

    /// <summary>
    /// A projector stub carrying nothing but the type the decorator reads for its tags.
    /// </summary>
    private static ISearchDocumentProjector Projector(EnumCoreContentType type)
    {
        Mock<ISearchDocumentProjector> mock = new();
        mock.SetupGet(projector => projector.Type).Returns(type);
        return mock.Object;
    }

    [Fact]
    public async Task SearchAsync_ShouldPassStraightThroughWithoutCaching()
    {
        // Arrange
        _innerMock
            .Setup(inner => inner.SearchAsync(EnumSearchScope.All, "fally", 20, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _catalog.SearchAsync(EnumSearchScope.All, "fally", skip: 20, take: 10);

        // Assert
        _innerMock.Verify(
            inner => inner.SearchAsync(EnumSearchScope.All, "fally", 20, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _capturedKey.Should().BeNull("a ranked page is never cached");
    }

    [Fact]
    public async Task CountAsync_OnAMiss_ShouldAskTheInnerCatalog()
    {
        // Arrange
        SearchCountsDto counts = new(new Dictionary<EnumSearchScope, int> { [EnumSearchScope.Articles] = 4 });
        _innerMock.Setup(inner => inner.CountAsync("fally", It.IsAny<CancellationToken>())).ReturnsAsync(counts);

        // Act
        SearchCountsDto result = await _catalog.CountAsync("fally");

        // Assert
        result.Should().BeSameAs(counts);
        _innerMock.Verify(inner => inner.CountAsync("fally", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CountAsync_ShouldKeyOnTheLowercasedTerm()
    {
        // Arrange
        _innerMock
            .Setup(inner => inner.CountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NoCounts);

        // Act
        await _catalog.CountAsync("Fally IPUPA");

        // Assert
        _capturedKey.Should().Be("search_counts:fally ipupa", "casing a query treats alike must share one entry");
    }

    [Fact]
    public async Task CountAsync_ShouldTagTheEntryWithEveryRegisteredType()
    {
        // Arrange
        _innerMock
            .Setup(inner => inner.CountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NoCounts);

        // Act
        await _catalog.CountAsync("fally");

        // Assert
        _capturedTags.Should().BeEquivalentTo([ContentCacheTags.Articles, ContentCacheTags.Videos]);
    }
}
