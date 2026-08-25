using _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;

/// <summary>
/// Unit tests for <see cref="PublicGetPopularTagsHandler"/>.
/// Uses a real <see cref="MemoryCache"/> instance so cache behaviour is tested
/// without mocking — verifying that the repository is only called once per
/// cache key regardless of how many times the handler is invoked.
/// A mock <see cref="IPopularTagsCacheInvalidator"/> is used to control the eviction token
/// and verify invalidation is NOT triggered by the query handler itself.
/// </summary>
public class PublicGetPopularTagsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IPopularTagsCacheInvalidator> _cacheInvalidatorMock;
    private readonly IMemoryCache _cache;
    private readonly PublicGetPopularTagsHandler _handler;

    public PublicGetPopularTagsHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _cacheInvalidatorMock = MockPopularTagsCacheInvalidator.Create();
        _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        _handler = new PublicGetPopularTagsHandler(
            _lookupRepositoryMock.Object,
            _cache,
            _cacheInvalidatorMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPopularTags()
    {
        // Arrange
        List<TagEntity> tags = TagFactory.CreateMany(5);
        _lookupRepositoryMock.SetupGetPopularTags(tags);

        var query = new PublicGetPopularTagsQuery(Limit: 5);

        // Act
        PublicGetPopularTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_WithNullLimit_ShouldReturnAllTags()
    {
        // Arrange
        List<TagEntity> tags = TagFactory.CreateMany(12);
        _lookupRepositoryMock.SetupGetPopularTags(tags);

        var query = new PublicGetPopularTagsQuery(Limit: null);

        // Act
        PublicGetPopularTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().HaveCount(12);

        _lookupRepositoryMock.Verify(
            x => x.GetPopularTagsAsync((int?)null, (EnumCoreContentType?)null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetPopularTags(new List<TagEntity>());

        var query = new PublicGetPopularTagsQuery();

        // Act
        PublicGetPopularTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CalledTwiceWithSameLimit_ShouldHitRepositoryOnlyOnce()
    {
        // Arrange — repository returns a list on the first call
        List<TagEntity> tags = TagFactory.CreateMany(3);
        _lookupRepositoryMock.SetupGetPopularTags(tags);

        var query = new PublicGetPopularTagsQuery(Limit: 3);

        // Act — call handler twice with the same limit
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);

        // Assert — repository must have been called exactly once (second call served from cache)
        _lookupRepositoryMock.Verify(
            x => x.GetPopularTagsAsync((int?)3, (EnumCoreContentType?)null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CalledWithDifferentLimits_ShouldHitRepositoryForEachLimit()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetPopularTags(TagFactory.CreateMany(5));

        // Act
        await _handler.Handle(new PublicGetPopularTagsQuery(Limit: 5), CancellationToken.None);
        await _handler.Handle(new PublicGetPopularTagsQuery(Limit: 10), CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            x => x.GetPopularTagsAsync((int?)5, It.IsAny<EnumCoreContentType?>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _lookupRepositoryMock.Verify(
            x => x.GetPopularTagsAsync((int?)10, It.IsAny<EnumCoreContentType?>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _lookupRepositoryMock.Verify(
            x =>
                x.GetPopularTagsAsync(
                    It.IsAny<int?>(),
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_ShouldPassLimitToRepository()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetPopularTags(new List<TagEntity>());

        var query = new PublicGetPopularTagsQuery(Limit: 10);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            x => x.GetPopularTagsAsync((int?)10, (EnumCoreContentType?)null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithArticleContentType_ShouldPassContentTypeToRepository()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetPopularTags(TagFactory.CreateMany(10));

        var query = new PublicGetPopularTagsQuery(Limit: 10, ContentType: EnumCoreContentType.Article);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            x => x.GetPopularTagsAsync((int?)10, EnumCoreContentType.Article, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithVideoContentType_ShouldPassContentTypeToRepository()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetPopularTags(TagFactory.CreateMany(10));

        var query = new PublicGetPopularTagsQuery(Limit: 10, ContentType: EnumCoreContentType.Video);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            x => x.GetPopularTagsAsync((int?)10, EnumCoreContentType.Video, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CalledWithDifferentContentTypes_ShouldHitRepositoryForEach()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetPopularTags(TagFactory.CreateMany(10));

        // Act
        await _handler.Handle(
            new PublicGetPopularTagsQuery(Limit: 10, ContentType: EnumCoreContentType.Article),
            CancellationToken.None
        );
        await _handler.Handle(
            new PublicGetPopularTagsQuery(Limit: 10, ContentType: EnumCoreContentType.Video),
            CancellationToken.None
        );

        // Assert
        _lookupRepositoryMock.Verify(
            x => x.GetPopularTagsAsync((int?)10, EnumCoreContentType.Article, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _lookupRepositoryMock.Verify(
            x => x.GetPopularTagsAsync((int?)10, EnumCoreContentType.Video, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _lookupRepositoryMock.Verify(
            x =>
                x.GetPopularTagsAsync(
                    It.IsAny<int?>(),
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_CalledTwiceWithSameContentType_ShouldHitRepositoryOnlyOnce()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetPopularTags(TagFactory.CreateMany(10));

        var query = new PublicGetPopularTagsQuery(Limit: 10, ContentType: EnumCoreContentType.Article);

        // Act
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);

        // Assert — second call served from cache
        _lookupRepositoryMock.Verify(
            x => x.GetPopularTagsAsync((int?)10, EnumCoreContentType.Article, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldNotCallInvalidate()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetPopularTags(TagFactory.CreateMany(3));

        var query = new PublicGetPopularTagsQuery(Limit: 3);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert — reads must never trigger cache invalidation
        _cacheInvalidatorMock.VerifyInvalidateNotCalled();
    }

    #endregion
}
