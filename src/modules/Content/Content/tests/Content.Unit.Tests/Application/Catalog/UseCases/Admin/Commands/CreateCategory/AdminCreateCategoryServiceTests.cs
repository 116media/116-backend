using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Unit tests for <see cref="AdminCreateCategoryService"/>: the slug and exclusive gates and the
/// staged category.
/// </summary>
public class AdminCreateCategoryServiceTests
{
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock = MockContentTypeRepository.Create();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly AdminCreateCategoryService _service;

    public AdminCreateCategoryServiceTests()
    {
        _service = new AdminCreateCategoryService(
            _contentTypeRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static AdminCreateCategoryCommand Command(Guid contentTypeId, bool isExclusive = false)
    {
        return new AdminCreateCategoryCommand(
            contentTypeId.ToString(),
            "Name",
            "slug",
            "Description",
            true,
            false,
            isExclusive
        );
    }

    [Fact]
    public async Task CreateAsync_WithAFreeSlug_ShouldStageTheCategory()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _categoryRepositoryMock.SetupGetBySlug("slug", null);

        // Act
        CategoryEntity category = await _service.CreateAsync(Command(contentType.Id), CancellationToken.None);

        // Assert
        category.Slug.Value.Should().Be("slug");
        category.ContentTypeId.Should().Be(contentType.Id);
        _categoryRepositoryMock.VerifyAddCalled();
    }

    [Fact]
    public async Task CreateAsync_WhenTheSlugIsTaken_ShouldThrowConflictException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _categoryRepositoryMock.SetupGetBySlug("slug", CategoryFactory.Create(contentType.Id));

        // Act
        Func<Task> act = async () => await _service.CreateAsync(Command(contentType.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateAsync_WhenExclusiveOnANonVideoType_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Article");
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _categoryRepositoryMock.SetupGetBySlug("slug", null);

        // Act
        Func<Task> act = async () =>
            await _service.CreateAsync(Command(contentType.Id, isExclusive: true), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateAsync_WhenExclusive_ShouldClearTheCurrentHolder()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        CategoryEntity current = CategoryFactory.Create(contentType.Id, isExclusive: true);
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _categoryRepositoryMock.SetupGetBySlug("slug", null);
        _categoryRepositoryMock.SetupGetExclusiveCategory(current);

        // Act
        CategoryEntity category = await _service.CreateAsync(
            Command(contentType.Id, isExclusive: true),
            CancellationToken.None
        );

        // Assert
        current.IsExclusive.Should().BeFalse();
        category.IsExclusive.Should().BeTrue();
    }
}
