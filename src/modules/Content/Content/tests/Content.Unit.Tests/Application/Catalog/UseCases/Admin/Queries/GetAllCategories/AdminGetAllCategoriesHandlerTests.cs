using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Queries.GetAllCategories;

/// <summary>
/// Unit tests for <see cref="AdminGetAllCategoriesHandler"/>.
/// </summary>
public class AdminGetAllCategoriesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminGetAllCategoriesHandler _handler;

    public AdminGetAllCategoriesHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminGetAllCategoriesHandler(
            _categoryRepositoryMock.Object,
            CreateCategoryDtoService(_fileStorageMock.Object)
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithMultipleCategories_ShouldReturnPaginatedResult()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        List<CategoryEntity> categories = CategoryFactory.CreateMany(contentType.Id, 3);
        int totalCount = 3;

        _categoryRepositoryMock.SetupGetAllAsync(categories, totalCount);

        var query = new AdminGetAllCategoriesQuery(PaginatedRequest: new PaginatedRequest(0, 10));

        // Act
        AdminGetAllCategoriesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Categories.Items.Should().HaveCount(3);
        result.Categories.Count.Should().Be(totalCount);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        _categoryRepositoryMock.SetupGetAllAsync(new List<CategoryEntity>(), 0);

        var query = new AdminGetAllCategoriesQuery(PaginatedRequest: new PaginatedRequest(0, 10));

        // Act
        AdminGetAllCategoriesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Categories.Items.Should().BeEmpty();
        result.Categories.Count.Should().Be(0);
    }

    #endregion
}
