using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Unit tests for <see cref="PublicGetAllTagsHandler"/>. Caching lives in the CQRS caching
/// decorator, so these cover the projection only; the conditional-caching contract the
/// decorator relies on is asserted on the query record.
/// </summary>
public class PublicGetAllTagsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly PublicGetAllTagsHandler _handler;

    public PublicGetAllTagsHandlerTests()
    {
        _tagRepositoryMock = MockTagRepository.Create();
        _handler = new PublicGetAllTagsHandler(_tagRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoSearch_ShouldReturnAllTags()
    {
        // Arrange
        List<TagEntity> tags = TagFactory.CreateMany(3);
        _tagRepositoryMock.SetupGetAllTags(tags);

        var query = new PublicGetAllTagsQuery(Search: null);

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassSearchToRepository()
    {
        // Arrange
        string searchTerm = TestConstants.Tag.ValidName;
        TagEntity tag = TagFactory.CreateDefault();
        _tagRepositoryMock.SetupGetAllTags(new List<TagEntity> { tag });

        var query = new PublicGetAllTagsQuery(Search: searchTerm);

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().ContainSingle();
        _tagRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
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
        _tagRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Article);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _tagRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
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
        _tagRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Video);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _tagRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
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
        _tagRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(ContentType: null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _tagRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
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
        _tagRepositoryMock.SetupGetAllTags(TagFactory.CreateMany(3));

        var query = new PublicGetAllTagsQuery(Limit: 5);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _tagRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
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
        _tagRepositoryMock.SetupGetAllTags(new List<TagEntity>());

        var query = new PublicGetAllTagsQuery();

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleTag_ShouldReturnMappedDto()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        _tagRepositoryMock.SetupGetAllTags(new List<TagEntity> { tag });

        var query = new PublicGetAllTagsQuery();

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().ContainSingle();
        result.Tags[0].Name.Should().Be(tag.Name);
        result.Tags[0].Slug.Should().Be(tag.Slug);
    }

    #endregion

    #region Query cache contract

    [Fact]
    public void IsCacheable_WithoutSearch_ShouldBeTrue()
    {
        new PublicGetAllTagsQuery(Search: null).IsCacheable.Should().BeTrue();
        new PublicGetAllTagsQuery(Search: "  ").IsCacheable.Should().BeTrue();
    }

    [Fact]
    public void IsCacheable_WithSearchTerm_ShouldBeFalse()
    {
        // Free-text search produces an unbounded key space, so those results are never stored.
        new PublicGetAllTagsQuery(Search: "afro")
            .IsCacheable.Should()
            .BeFalse();
    }

    [Fact]
    public void CacheKey_WithSameArguments_ShouldBeStable()
    {
        var first = new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Article, Limit: 5);
        var second = new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Article, Limit: 5);

        first.CacheKey.Should().Be(second.CacheKey);
    }

    [Fact]
    public void CacheKey_WithDifferentArguments_ShouldDiffer()
    {
        var baseline = new PublicGetAllTagsQuery();

        // Every parameter that changes the result participates in the key.
        new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Article)
            .CacheKey.Should()
            .NotBe(baseline.CacheKey);
        new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Video)
            .CacheKey.Should()
            .NotBe(new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Article).CacheKey);
        new PublicGetAllTagsQuery(Limit: 5).CacheKey.Should().NotBe(baseline.CacheKey);
        new PublicGetAllTagsQuery(Limit: 10).CacheKey.Should().NotBe(new PublicGetAllTagsQuery(Limit: 5).CacheKey);
    }

    [Fact]
    public void CacheTags_ShouldCarryTheTagsTag()
    {
        new PublicGetAllTagsQuery().CacheTags.Should().ContainSingle().Which.Should().Be(ContentCacheTags.Tags);
    }

    #endregion
}
