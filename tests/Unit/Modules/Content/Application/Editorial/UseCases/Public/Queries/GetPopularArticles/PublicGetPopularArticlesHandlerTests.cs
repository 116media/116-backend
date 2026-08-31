using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Unit tests for <see cref="PublicGetPopularArticlesHandler"/>. Caching lives in the
/// CQRS caching decorator, so these cover the projection only; the cache-key contract
/// the decorator relies on is asserted on the query record.
/// </summary>
public class PublicGetPopularArticlesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetPopularArticlesHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPopularArticlesHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        FileEntity coverFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupGetById(coverFile);
        _handler = new PublicGetPopularArticlesHandler(_articleRepositoryMock.Object, _fileRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPopularArticlesExist_ShouldReturnMappedList()
    {
        // Arrange
        List<ArticleEntity> articles = ArticleFactory.CreateManyPublished(CategoryId, 3);
        _articleRepositoryMock.SetupGetPopularArticlesAsync(articles);

        var query = new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: null);

        // Act
        PublicGetPopularArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Count.Should().Be(articles.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPopularArticlesExist_ShouldReturnEmptyList()
    {
        // Arrange
        _articleRepositoryMock.SetupGetPopularArticlesAsync(new List<ArticleEntity>());

        var query = new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: null);

        // Act
        PublicGetPopularArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPassArgumentsToRepository()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var excludeId = Guid.NewGuid();
        _articleRepositoryMock.SetupGetPopularArticlesAsync(ArticleFactory.CreateManyPublished(CategoryId, 1));

        var query = new PublicGetPopularArticlesQuery(Limit: 7, CategoryId: categoryId, ExcludeId: excludeId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _articleRepositoryMock.Verify(
            x => x.GetPopularArticlesAsync(7, categoryId, excludeId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public void CacheKey_WithSameArguments_ShouldBeStable()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var first = new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: categoryId, ExcludeId: null);
        var second = new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: categoryId, ExcludeId: null);

        // Assert
        first.CacheKey.Should().Be(second.CacheKey);
    }

    [Fact]
    public void CacheKey_WithDifferentArguments_ShouldDiffer()
    {
        // Arrange
        var baseline = new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: null);

        // Assert — every parameter participates in the key
        new PublicGetPopularArticlesQuery(Limit: 7, CategoryId: null, ExcludeId: null)
            .CacheKey.Should()
            .NotBe(baseline.CacheKey);
        new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: Guid.NewGuid(), ExcludeId: null)
            .CacheKey.Should()
            .NotBe(baseline.CacheKey);
        new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: Guid.NewGuid())
            .CacheKey.Should()
            .NotBe(baseline.CacheKey);
    }

    [Fact]
    public void CacheTags_ShouldCarryThePopularArticlesTag()
    {
        // Arrange
        var query = new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: null);

        // Assert
        query.CacheTags.Should().ContainSingle().Which.Should().Be(ContentCacheTags.PopularArticles);
    }
}
