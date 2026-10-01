using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Unit tests for <see cref="AdminUpdateCategoryService"/>: the slug, exclusive and default-for-lyrics gates.
/// </summary>
public class AdminUpdateCategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock = MockContentTypeRepository.Create();
    private readonly AdminUpdateCategoryService _service;

    public AdminUpdateCategoryServiceTests()
    {
        _service = new AdminUpdateCategoryService(
            _categoryRepositoryMock.Object,
            _contentTypeRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private CategoryEntity Arrange(string contentTypeName, bool isActive = true)
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create(contentTypeName);
        CategoryEntity category = isActive
            ? CategoryFactory.Create(contentType.Id)
            : CategoryFactory.CreateInactive(contentType.Id);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _categoryRepositoryMock.SetupGetBySlug("slug", null);
        return category;
    }

    private static AdminUpdateCategoryCommand Command(
        Guid id,
        bool isExclusive = false,
        bool isDefaultForLyrics = false
    )
    {
        return new AdminUpdateCategoryCommand(
            id.ToString(),
            "Name",
            "slug",
            "Description",
            false,
            isExclusive,
            isDefaultForLyrics
        );
    }

    [Fact]
    public async Task EnsureUpdatableAsync_WithAFreeSlug_ShouldReturnTheCategory()
    {
        // Arrange
        CategoryEntity category = Arrange("Video");

        // Act
        CategoryEntity result = await _service.EnsureUpdatableAsync(Command(category.Id), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(category);
    }

    [Fact]
    public async Task EnsureUpdatableAsync_WhenTheSlugBelongsToAnotherCategory_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = Arrange("Video");
        _categoryRepositoryMock.SetupGetBySlug("slug", CategoryFactory.Create(category.ContentTypeId));

        // Act
        Func<Task> act = async () => await _service.EnsureUpdatableAsync(Command(category.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task EnsureUpdatableAsync_WhenExclusiveOnAnArticleCategory_ShouldThrowBadRequestException()
    {
        // Arrange
        CategoryEntity category = Arrange("Article");

        // Act
        Func<Task> act = async () =>
            await _service.EnsureUpdatableAsync(Command(category.Id, isExclusive: true), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task EnsureUpdatableAsync_WhenDefaultForLyricsOnAnInactiveCategory_ShouldThrowBadRequestException()
    {
        // Arrange
        CategoryEntity category = Arrange("Lyrics", isActive: false);

        // Act
        Func<Task> act = async () =>
            await _service.EnsureUpdatableAsync(Command(category.Id, isDefaultForLyrics: true), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
