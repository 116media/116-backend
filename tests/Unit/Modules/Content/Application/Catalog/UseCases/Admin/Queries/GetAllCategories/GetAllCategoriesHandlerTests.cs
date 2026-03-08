using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories;

/// <summary>
/// Unit tests for <see cref="GetAllCategoriesHandler"/>.
/// </summary>
public class GetAllCategoriesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetAllCategoriesHandler _handler;

    public GetAllCategoriesHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _handler = new GetAllCategoriesHandler(_categoryRepositoryMock.Object, Mapper);
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

        var query = new GetAllCategoriesQuery(PaginatedRequest: new PaginatedRequest(PageIndex: 0, PageSize: 10));

        // Act
        GetAllCategoriesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Categories.Should().NotBeNull();
        result.Categories.Items.Should().HaveCount(3);
        result.Categories.Count.Should().Be(totalCount);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        _categoryRepositoryMock.SetupGetAllAsync(new List<CategoryEntity>(), 0);

        var query = new GetAllCategoriesQuery(PaginatedRequest: new PaginatedRequest(PageIndex: 0, PageSize: 10));

        // Act
        GetAllCategoriesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Categories.Items.Should().BeEmpty();
        result.Categories.Count.Should().Be(0);
    }

    #endregion
}
