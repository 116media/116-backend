using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory;
using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Application.Shared.Repositories;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory;

/// <summary>
/// Unit tests for <see cref="PublicGetExclusiveCategoryHandler"/>.
/// </summary>
public class PublicGetExclusiveCategoryHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetExclusiveCategoryHandler _handler;

    public PublicGetExclusiveCategoryHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new PublicGetExclusiveCategoryHandler(
            _categoryRepositoryMock.Object,
            _videoRepositoryMock.Object,
            CreateCategoryDtoFactory(_fileStorageMock.Object),
            new VideoDtoFactory(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupFactory()
            ),
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithExclusiveCategory_ShouldReturnCategoryAndVideos()
    {
        // Arrange
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity exclusive = CategoryFactory.Create(videoType, isExclusive: true);
        List<VideoEntity> videos = VideoFactory.CreateMany(exclusive.Id, 3);

        _categoryRepositoryMock.SetupGetExclusiveCategory(exclusive);
        _videoRepositoryMock.SetupGetAllAsync(videos, totalCount: 3);

        var query = new PublicGetExclusiveCategoryQuery(PaginatedRequest: new PaginatedRequest(0, 10));

        // Act
        PublicGetExclusiveCategoryResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Category.IsExclusive.Should().BeTrue();
        result.Videos.Items.Should().HaveCount(3);
        result.Videos.Count.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithExclusiveCategoryAndNoVideos_ShouldReturnEmptyVideoList()
    {
        // Arrange
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity exclusive = CategoryFactory.Create(videoType, isExclusive: true);

        _categoryRepositoryMock.SetupGetExclusiveCategory(exclusive);
        _videoRepositoryMock.SetupGetAllAsync(new List<VideoEntity>(), totalCount: 0);

        var query = new PublicGetExclusiveCategoryQuery(PaginatedRequest: new PaginatedRequest(0, 10));

        // Act
        PublicGetExclusiveCategoryResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Category.IsExclusive.Should().BeTrue();
        result.Videos.Items.Should().BeEmpty();
        result.Videos.Count.Should().Be(0);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WithNoExclusiveCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new PublicGetExclusiveCategoryQuery(PaginatedRequest: new PaginatedRequest(0, 10));

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
