using _116.Content.Application.Catalog.UseCases.Public.Queries.GetPublicCategories;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Public.Queries.GetPublicCategories;

/// <summary>
/// Unit tests for <see cref="GetPublicCategoriesHandler"/>.
/// </summary>
public class GetPublicCategoriesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetPublicCategoriesHandler _handler;

    public GetPublicCategoriesHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _handler = new GetPublicCategoriesHandler(_categoryRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoFilter_ShouldReturnAllActiveCategories()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        List<CategoryEntity> categories = CategoryFactory.CreateMany(contentType.Id, 3);

        _categoryRepositoryMock.SetupGetActiveByContentType(null, categories);

        var query = new GetPublicCategoriesQuery(ContentTypeId: null);

        // Act
        GetPublicCategoriesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Categories.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithContentTypeIdFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        List<CategoryEntity> filtered = CategoryFactory.CreateMany(contentType.Id, 2);

        _categoryRepositoryMock.SetupGetActiveByContentType(contentType.Id, filtered);

        var query = new GetPublicCategoriesQuery(ContentTypeId: contentType.Id);

        // Act
        GetPublicCategoriesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Categories.Should().HaveCount(2);

        _categoryRepositoryMock.Verify(
            x => x.GetActiveByContentTypeAsync(contentType.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenNoCategoriesExist_ShouldReturnEmptyList()
    {
        // Arrange
        _categoryRepositoryMock.SetupGetActiveByContentType(null, new List<CategoryEntity>());

        var query = new GetPublicCategoriesQuery(ContentTypeId: null);

        // Act
        GetPublicCategoriesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Categories.Should().BeEmpty();
    }

    #endregion
}
