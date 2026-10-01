using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory;

/// <summary>
/// Unit tests for <see cref="AdminSetExclusiveCategoryService"/>: the state and type gates.
/// </summary>
public class AdminSetExclusiveCategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock = MockContentTypeRepository.Create();
    private readonly AdminSetExclusiveCategoryService _service;

    public AdminSetExclusiveCategoryServiceTests()
    {
        _service = new AdminSetExclusiveCategoryService(
            _categoryRepositoryMock.Object,
            _contentTypeRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task EnsureExclusivableAsync_WithAnActiveVideoCategory_ShouldReturnIt()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);

        // Act
        CategoryEntity result = await _service.EnsureExclusivableAsync(category.Id, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(category);
    }

    [Fact]
    public async Task EnsureExclusivableAsync_WhenInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        CategoryEntity category = CategoryFactory.CreateInactive(contentType.Id);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);

        // Act
        Func<Task> act = async () => await _service.EnsureExclusivableAsync(category.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task EnsureExclusivableAsync_WhenNotAVideoCategory_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Article");
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);

        // Act
        Func<Task> act = async () => await _service.EnsureExclusivableAsync(category.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
