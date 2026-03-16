using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;

/// <summary>
/// Unit tests for <see cref="PublicGetPublishedArticlesHandler"/>.
/// </summary>
public class PublicGetPublishedArticlesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly PublicGetPublishedArticlesHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPublishedArticlesHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _handler = new PublicGetPublishedArticlesHandler(_articleRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenPublishedArticlesExist_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<ArticleEntity> articles = ArticleFactory.CreateManyPublished(CategoryId, 3);
        var query = new PublicGetPublishedArticlesQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            CategoryId: null,
            TagSlug: null
        );

        _articleRepositoryMock.SetupGetAllAsync(articles, articles.Count);

        // Act
        PublicGetPublishedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Items.Count().Should().Be(articles.Count);
        result.Articles.Count.Should().Be((long)articles.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPublishedArticlesExist_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var query = new PublicGetPublishedArticlesQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            CategoryId: null,
            TagSlug: null
        );

        _articleRepositoryMock.SetupGetAllAsync(new List<ArticleEntity>(), 0);

        // Act
        PublicGetPublishedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Should().BeEmpty();
        result.Articles.Count.Should().Be(0);
    }
}
