using _116.Content.Application.Catalog.UseCases.Public.Queries.GetActiveCategories;
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
/// Unit tests for <see cref="PublicGetActiveCategoriesHandler"/>.
/// </summary>
public class PublicGetActiveCategoriesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly PublicGetActiveCategoriesHandler _handler;

    public PublicGetActiveCategoriesHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _handler = new PublicGetActiveCategoriesHandler(_categoryRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoFilter_ShouldReturnAllActiveCategories()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        List<CategoryEntity> categories = CategoryFactory.CreateMany(contentType.Id, 3);

        _categoryRepositoryMock.SetupGetActiveByContentType(null, categories);

        var query = new PublicGetActiveCategoriesQuery(ContentTypeId: null);

        // Act
        PublicGetActiveCategoriesResult result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new PublicGetActiveCategoriesQuery(ContentTypeId: contentType.Id);

        // Act
        PublicGetActiveCategoriesResult result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new PublicGetActiveCategoriesQuery(ContentTypeId: null);

        // Act
        PublicGetActiveCategoriesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Categories.Should().BeEmpty();
    }

    #endregion
}
