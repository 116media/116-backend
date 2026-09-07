using _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;

/// <summary>
/// Unit tests for <see cref="PublicGetPopularTagsHandler"/>. Caching lives in the CQRS
/// caching decorator, so these cover the projection only; the cache-key contract the
/// decorator relies on is asserted on the query record.
/// </summary>
public class PublicGetPopularTagsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly PublicGetPopularTagsHandler _handler;

    public PublicGetPopularTagsHandlerTests()
    {
        _tagRepositoryMock = MockTagRepository.Create();
        _handler = new PublicGetPopularTagsHandler(_tagRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPopularTags()
    {
        // Arrange
        List<TagEntity> tags = TagFactory.CreateMany(5);
        _tagRepositoryMock.SetupGetPopularTags(tags);

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
        _tagRepositoryMock.SetupGetPopularTags(tags);

        var query = new PublicGetPopularTagsQuery(Limit: null);

        // Act
        PublicGetPopularTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().HaveCount(12);

        _tagRepositoryMock.Verify(
            x => x.GetPopularAsync((int?)null, (EnumCoreContentType?)null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _tagRepositoryMock.SetupGetPopularTags(new List<TagEntity>());

        var query = new PublicGetPopularTagsQuery();

        // Act
        PublicGetPopularTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPassLimitToRepository()
    {
        // Arrange
        _tagRepositoryMock.SetupGetPopularTags(new List<TagEntity>());

        var query = new PublicGetPopularTagsQuery(Limit: 7);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _tagRepositoryMock.Verify(
            x => x.GetPopularAsync((int?)7, (EnumCoreContentType?)null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithArticleContentType_ShouldPassContentTypeToRepository()
    {
        // Arrange
        _tagRepositoryMock.SetupGetPopularTags(TagFactory.CreateMany(10));

        var query = new PublicGetPopularTagsQuery(ContentType: EnumCoreContentType.Article);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _tagRepositoryMock.Verify(
            x => x.GetPopularAsync(It.IsAny<int?>(), EnumCoreContentType.Article, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithVideoContentType_ShouldPassContentTypeToRepository()
    {
        // Arrange
        _tagRepositoryMock.SetupGetPopularTags(TagFactory.CreateMany(10));

        var query = new PublicGetPopularTagsQuery(ContentType: EnumCoreContentType.Video);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _tagRepositoryMock.Verify(
            x => x.GetPopularAsync(It.IsAny<int?>(), EnumCoreContentType.Video, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Query cache contract

    [Fact]
    public void CacheKey_WithSameArguments_ShouldBeStable()
    {
        var first = new PublicGetPopularTagsQuery(Limit: 5, ContentType: EnumCoreContentType.Article);
        var second = new PublicGetPopularTagsQuery(Limit: 5, ContentType: EnumCoreContentType.Article);

        first.CacheKey.Should().Be(second.CacheKey);
    }

    [Fact]
    public void CacheKey_WithDifferentArguments_ShouldDiffer()
    {
        var baseline = new PublicGetPopularTagsQuery();

        // Every parameter that changes the result participates in the key.
        new PublicGetPopularTagsQuery(Limit: 5)
            .CacheKey.Should()
            .NotBe(baseline.CacheKey);
        new PublicGetPopularTagsQuery(Limit: 10)
            .CacheKey.Should()
            .NotBe(new PublicGetPopularTagsQuery(Limit: 5).CacheKey);
        new PublicGetPopularTagsQuery(ContentType: EnumCoreContentType.Article)
            .CacheKey.Should()
            .NotBe(baseline.CacheKey);
        new PublicGetPopularTagsQuery(ContentType: EnumCoreContentType.Video)
            .CacheKey.Should()
            .NotBe(new PublicGetPopularTagsQuery(ContentType: EnumCoreContentType.Article).CacheKey);
    }

    [Fact]
    public void CacheTags_ShouldCarryTheTagsTag()
    {
        new PublicGetPopularTagsQuery().CacheTags.Should().ContainSingle().Which.Should().Be(ContentCacheTags.Tags);
    }

    #endregion
}
