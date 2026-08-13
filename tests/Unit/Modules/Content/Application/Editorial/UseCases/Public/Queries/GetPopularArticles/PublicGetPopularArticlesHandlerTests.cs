using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Unit tests for <see cref="PublicGetPopularArticlesHandler"/>.
/// </summary>
public class PublicGetPopularArticlesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IPopularArticlesCacheInvalidator> _cacheInvalidatorMock;
    private readonly IMemoryCache _cache;
    private readonly PublicGetPopularArticlesHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPopularArticlesHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _cacheInvalidatorMock = MockPopularArticlesCacheInvalidator.Create();
        _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        FileEntity coverFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupGetById(coverFile);
        _handler = new PublicGetPopularArticlesHandler(
            _articleRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _cache,
            _cacheInvalidatorMock.Object,
            Mapper
        );
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
    public async Task Handle_CalledTwiceWithSameArgs_ShouldHitRepositoryOnce()
    {
        // Arrange
        _articleRepositoryMock.SetupGetPopularArticlesAsync(ArticleFactory.CreateManyPublished(CategoryId, 3));
        var query = new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: null);

        // Act
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _articleRepositoryMock.Verify(
            x => x.GetPopularArticlesAsync(5, null, null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CalledWithDifferentExcludeId_ShouldHitRepositoryTwice()
    {
        // Arrange
        _articleRepositoryMock.SetupGetPopularArticlesAsync(ArticleFactory.CreateManyPublished(CategoryId, 3));
        Guid firstExcludeId = Guid.NewGuid();
        Guid secondExcludeId = Guid.NewGuid();

        // Act
        await _handler.Handle(
            new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: firstExcludeId),
            CancellationToken.None
        );
        await _handler.Handle(
            new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: secondExcludeId),
            CancellationToken.None
        );

        // Assert
        _articleRepositoryMock.Verify(
            x => x.GetPopularArticlesAsync(5, null, firstExcludeId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _articleRepositoryMock.Verify(
            x => x.GetPopularArticlesAsync(5, null, secondExcludeId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _articleRepositoryMock.Verify(
            x =>
                x.GetPopularArticlesAsync(
                    It.IsAny<int>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_ShouldNotCallInvalidate()
    {
        // Arrange
        _articleRepositoryMock.SetupGetPopularArticlesAsync(ArticleFactory.CreateManyPublished(CategoryId, 3));

        // Act
        await _handler.Handle(
            new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: null),
            CancellationToken.None
        );

        // Assert
        _cacheInvalidatorMock.VerifyInvalidateNotCalled();
    }
}
