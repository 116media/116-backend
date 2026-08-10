using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Cache;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Unit tests for <see cref="PublicGetAllTagsHandler"/>.
/// Uses a real <see cref="MemoryCache"/> instance and a real <see cref="PopularTagsCacheInvalidator"/>
/// so that search-aware caching behaviour is exercised without mocking — verifying that the
/// repository is called only once per (limit, contentType) cache key for unfiltered requests,
/// that search requests bypass the cache, and that invalidation forces a fresh repository read.
/// </summary>
public class PublicGetAllTagsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly IMemoryCache _cache;
    private readonly PopularTagsCacheInvalidator _cacheInvalidator;
    private readonly PublicGetAllTagsHandler _handler;

    public PublicGetAllTagsHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        _cacheInvalidator = new PopularTagsCacheInvalidator();
        _handler = new PublicGetAllTagsHandler(_lookupRepositoryMock.Object, _cache, _cacheInvalidator, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoSearch_ShouldReturnAllTags()
    {
        // Arrange
        List<TagEntity> tags = TagFactory.CreateMany(3);
        _lookupRepositoryMock.SetupGetAllTags(tags);

        var query = new PublicGetAllTagsQuery(Search: null);

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassSearchToRepository()
    {
        // Arrange
        string searchTerm = TestConstants.Tag.ValidName;
        TagEntity tag = TagFactory.CreateDefault();
        _lookupRepositoryMock.SetupGetAllTags(new List<TagEntity> { tag });

        var query = new PublicGetAllTagsQuery(Search: searchTerm);

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().ContainSingle();
        _lookupRepositoryMock.Verify(
            x =>
                x.GetAllTagsAsync(
                    searchTerm,
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithArticleContentType_ShouldPassContentTypeToRepository()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Article);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            x =>
                x.GetAllTagsAsync(
                    It.IsAny<string?>(),
                    EnumCoreContentType.Article,
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithVideoContentType_ShouldPassContentTypeToRepository()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Video);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            x =>
                x.GetAllTagsAsync(
                    It.IsAny<string?>(),
                    EnumCoreContentType.Video,
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithNullContentType_ShouldPassNullToRepository()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(ContentType: null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            x =>
                x.GetAllTagsAsync(
                    It.IsAny<string?>(),
                    (EnumCoreContentType?)null,
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithLimit_ShouldPassLimitToRepository()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(Limit: 5);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            x =>
                x.GetAllTagsAsync(
                    It.IsAny<string?>(),
                    It.IsAny<EnumCoreContentType?>(),
                    (int?)5,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(new List<TagEntity>());

        var query = new PublicGetAllTagsQuery();

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleTag_ShouldReturnMappedDto()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        _lookupRepositoryMock.SetupGetAllTags(new List<TagEntity> { tag });

        var query = new PublicGetAllTagsQuery();

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().ContainSingle();
        result.Tags[0].Name.Should().Be(tag.Name);
        result.Tags[0].Slug.Should().Be(tag.Slug);
    }

    #endregion

    #region Caching

    [Fact]
    public async Task Handle_CalledTwiceWithSameNullSearchQuery_ShouldHitRepositoryOnlyOnce()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(Search: null);

        // Act — two identical unfiltered calls; the second must be served from cache
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            x =>
                x.GetAllTagsAsync(
                    It.IsAny<string?>(),
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CalledTwiceWithSearchTerm_ShouldBypassCacheAndHitRepositoryEachTime()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(Search: "afro");

        // Act — search requests never cache, so both calls hit the repository
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            x =>
                x.GetAllTagsAsync(
                    "afro",
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_CalledWithDifferentContentTypes_ShouldHitRepositoryForEach()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        // Act — Article and Video produce different cache keys
        await _handler.Handle(
            new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Article),
            CancellationToken.None
        );
        await _handler.Handle(
            new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Video),
            CancellationToken.None
        );

        // Assert — repository called once per unique content type
        _lookupRepositoryMock.Verify(
            x =>
                x.GetAllTagsAsync(
                    It.IsAny<string?>(),
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_CalledWithDifferentLimits_ShouldHitRepositoryForEach()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        // Act — limit is part of the cache key, so 5 and 10 cache independently
        await _handler.Handle(new PublicGetAllTagsQuery(Limit: 5), CancellationToken.None);
        await _handler.Handle(new PublicGetAllTagsQuery(Limit: 10), CancellationToken.None);

        // Assert — repository called once per unique limit
        _lookupRepositoryMock.Verify(
            x =>
                x.GetAllTagsAsync(
                    It.IsAny<string?>(),
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_AfterInvalidation_ShouldHitRepositoryAgain()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(Search: null);

        // Act — cache the result, then invalidate the shared token before the next read
        await _handler.Handle(query, CancellationToken.None);
        _cacheInvalidator.Invalidate();
        await _handler.Handle(query, CancellationToken.None);

        // Assert — invalidation evicts the entry, forcing a fresh repository read
        _lookupRepositoryMock.Verify(
            x =>
                x.GetAllTagsAsync(
                    It.IsAny<string?>(),
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }

    #endregion
}
