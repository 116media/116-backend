using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllVideos;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Queries.GetAllVideos;

/// <summary>
/// Unit tests for <see cref="AdminGetAllVideosHandler"/>.
/// </summary>
public class AdminGetAllVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminGetAllVideosHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetAllVideosHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminGetAllVideosHandler(
            _videoRepositoryMock.Object,
            new VideoDtoService(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupService()
            )
        );
    }

    [Fact]
    public async Task Handle_WhenVideosExist_ShouldReturnPaginatedResult()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        List<VideoEntity> videos = VideoFactory.CreateManyWithCategory(category, 3);
        var query = new AdminGetAllVideosQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Status: null,
            CategoryId: null
        );

        _videoRepositoryMock.SetupGetAllAsync(videos, videos.Count);

        // Act
        AdminGetAllVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Items.Should().HaveCount(videos.Count);
        result.Videos.Count.Should().Be((long)videos.Count);
    }

    [Fact]
    public async Task Handle_WhenNoVideosExist_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var query = new AdminGetAllVideosQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Status: null,
            CategoryId: null
        );

        _videoRepositoryMock.SetupGetAllAsync(new List<VideoEntity>(), 0);

        // Act
        AdminGetAllVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Items.Should().BeEmpty();
        result.Videos.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenStatusFilterProvided_ShouldReturnFilteredResults()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        List<VideoEntity> published = VideoFactory.CreateManyWithCategory(category, 2);
        var query = new AdminGetAllVideosQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Status: EnumContentStatus.Published,
            CategoryId: null
        );

        _videoRepositoryMock.SetupGetAllAsync(published, published.Count);

        // Act
        AdminGetAllVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Items.Should().HaveCount(published.Count);
        result.Videos.Count.Should().Be((long)published.Count);
    }
}
