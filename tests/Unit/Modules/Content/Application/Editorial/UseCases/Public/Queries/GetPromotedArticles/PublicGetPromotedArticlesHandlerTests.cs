using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;

/// <summary>
/// Unit tests for <see cref="PublicGetPromotedArticlesHandler"/>.
/// </summary>
public class PublicGetPromotedArticlesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetPromotedArticlesHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPromotedArticlesHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        FileEntity coverFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupGetById(coverFile);
        _handler = new PublicGetPromotedArticlesHandler(
            _articleRepositoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenPromotedArticlesExist_ShouldReturnArticleList()
    {
        // Arrange
        List<ArticleEntity> promoted = ArticleFactory.CreateMany(CategoryId, 2);
        var query = new PublicGetPromotedArticlesQuery();

        _articleRepositoryMock.SetupGetPromotedAsync(promoted);

        // Act
        PublicGetPromotedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().NotBeNull();
        result.Articles.Count.Should().Be(promoted.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPromotedArticlesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new PublicGetPromotedArticlesQuery();

        _articleRepositoryMock.SetupGetPromotedAsync(new List<ArticleEntity>());

        // Act
        PublicGetPromotedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Should().BeEmpty();
    }
}
