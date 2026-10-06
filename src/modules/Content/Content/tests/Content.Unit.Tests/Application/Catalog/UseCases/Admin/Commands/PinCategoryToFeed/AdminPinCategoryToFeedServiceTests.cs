using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed;

/// <summary>
/// Unit tests for <see cref="AdminPinCategoryToFeedService"/>: the state, type and volume gates, the
/// FIFO eviction and the pin.
/// </summary>
public class AdminPinCategoryToFeedServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock = MockContentTypeRepository.Create();
    private readonly Mock<IVideoRepository> _videoRepositoryMock = MockVideoRepository.Create();
    private readonly AdminPinCategoryToFeedService _service;

    public AdminPinCategoryToFeedServiceTests()
    {
        _service = new AdminPinCategoryToFeedService(
            _categoryRepositoryMock.Object,
            _contentTypeRepositoryMock.Object,
            _videoRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n(),
            TimeProvider.System
        );
    }

    private CategoryEntity ArrangeEligible(IReadOnlyList<CategoryEntity>? pinned = null)
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _videoRepositoryMock.SetupCountPublishedByCategory(category.Id, EditorialFeedConstants.MinVideosToPinToFeed);
        _categoryRepositoryMock.SetupGetPinnedToFeedCategories(pinned ?? []);
        return category;
    }

    [Fact]
    public async Task PinAsync_WithAnEligibleCategory_ShouldPinIt()
    {
        // Arrange
        CategoryEntity category = ArrangeEligible();

        // Act
        CategoryEntity result = await _service.PinAsync(category.Id, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(category);
        category.PinnedToFeedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PinAsync_WhenTheCapIsReached_ShouldUnpinTheOldest()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        var now = DateTimeOffset.UtcNow;
        List<CategoryEntity> pinned =
        [
            .. Enumerable
                .Range(0, CatalogFeedConstants.MaxPinnedCategoriesPerContentType)
                .Select(i => CategoryFactory.CreatePinned(contentType.Id, now.AddDays(-i))),
        ];
        CategoryEntity oldest = pinned[^1];
        CategoryEntity category = ArrangeEligible(pinned);

        // Act
        await _service.PinAsync(category.Id, CancellationToken.None);

        // Assert
        oldest.PinnedToFeedAt.Should().BeNull();
        category.PinnedToFeedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PinAsync_WhenTheCategoryIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        CategoryEntity category = CategoryFactory.CreateInactive(contentType.Id);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);

        // Act
        Func<Task> act = async () => await _service.PinAsync(category.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task PinAsync_WhenTheCategoryIsNotAVideoCategory_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Article");
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);

        // Act
        Func<Task> act = async () => await _service.PinAsync(category.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task PinAsync_WhenTooFewVideosArePublished_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _videoRepositoryMock.SetupCountPublishedByCategory(
            category.Id,
            EditorialFeedConstants.MinVideosToPinToFeed - 1
        );

        // Act
        Func<Task> act = async () => await _service.PinAsync(category.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
