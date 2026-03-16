using _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedArticles;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedArticles;

/// <summary>
/// Unit tests for <see cref="PublicGetFeaturedArticlesHandler"/>.
/// </summary>
public class PublicGetFeaturedArticlesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly PublicGetFeaturedArticlesHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetFeaturedArticlesHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _handler = new PublicGetFeaturedArticlesHandler(_articleRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenFeaturedArticlesExist_ShouldReturnArticleList()
    {
        // Arrange
        List<ArticleEntity> featured = ArticleFactory.CreateMany(CategoryId, 2);
        var query = new PublicGetFeaturedArticlesQuery();

        _articleRepositoryMock.SetupGetFeaturedAsync(featured);

        // Act
        PublicGetFeaturedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().NotBeNull();
        result.Articles.Count.Should().Be(featured.Count);
    }

    [Fact]
    public async Task Handle_WhenNoFeaturedArticlesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new PublicGetFeaturedArticlesQuery();

        _articleRepositoryMock.SetupGetFeaturedAsync(new List<ArticleEntity>());

        // Act
        PublicGetFeaturedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Should().BeEmpty();
    }
}
