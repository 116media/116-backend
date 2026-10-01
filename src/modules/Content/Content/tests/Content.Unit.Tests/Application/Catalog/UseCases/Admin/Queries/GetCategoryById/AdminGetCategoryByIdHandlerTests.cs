using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Storage.Contracts.Application.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;

/// <summary>
/// Unit tests for <see cref="AdminGetCategoryByIdHandler"/>.
/// </summary>
public class AdminGetCategoryByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminGetCategoryByIdHandler _handler;

    public AdminGetCategoryByIdHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminGetCategoryByIdHandler(
            _categoryRepositoryMock.Object,
            CreateCategoryDtoService(_fileStorageMock.Object)
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenCategoryFound_ShouldReturnDto()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.CreateDefault(contentType.Id);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        var query = new AdminGetCategoryByIdQuery(Id: category.Id);

        // Act
        AdminGetCategoryByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Category.Id.Should().Be(category.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        var query = new AdminGetCategoryByIdQuery(Id: nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
