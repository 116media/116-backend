using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArticles;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Queries.GetAllArticles;

/// <summary>
/// Unit tests for <see cref="AdminGetAllArticlesHandler"/>.
/// </summary>
public class AdminGetAllArticlesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminGetAllArticlesHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetAllArticlesHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        FileReferenceDto coverFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupResolve(coverFile);
        _handler = new AdminGetAllArticlesHandler(
            _articleRepositoryMock.Object,
            _fileStorageMock.Object,
            Mapper,
            CreateContentLookupService()
        );
    }

    [Fact]
    public async Task Handle_WhenArticlesExist_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<ArticleEntity> articles = ArticleFactory.CreateMany(CategoryId, 3);
        var query = new AdminGetAllArticlesQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Status: null,
            CategoryId: null
        );

        _articleRepositoryMock.SetupGetAllAsync(articles, articles.Count);

        // Act
        AdminGetAllArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Should().HaveCount(articles.Count);
        result.Articles.Count.Should().Be((long)articles.Count);
    }

    [Fact]
    public async Task Handle_WhenNoArticlesExist_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var query = new AdminGetAllArticlesQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Status: null,
            CategoryId: null
        );

        _articleRepositoryMock.SetupGetAllAsync(new List<ArticleEntity>(), 0);

        // Act
        AdminGetAllArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Should().BeEmpty();
        result.Articles.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenStatusFilterProvided_ShouldReturnFilteredResults()
    {
        // Arrange
        List<ArticleEntity> published = ArticleFactory.CreateManyPublished(CategoryId, 2);
        var query = new AdminGetAllArticlesQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Status: EnumContentStatus.Published,
            CategoryId: null
        );

        _articleRepositoryMock.SetupGetAllAsync(published, published.Count);

        // Act
        AdminGetAllArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Should().HaveCount(published.Count);
        result.Articles.Count.Should().Be((long)published.Count);
    }
}
