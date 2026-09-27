using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
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
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;

/// <summary>
/// Unit tests for <see cref="PublicGetPublishedVideosHandler"/>.
/// </summary>
public class PublicGetPublishedVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetPublishedVideosHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPublishedVideosHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new PublicGetPublishedVideosHandler(
            _videoRepositoryMock.Object,
            new VideoDtoFactory(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupFactory()
            )
        );
    }

    [Fact]
    public async Task Handle_WhenPublishedVideosExist_ShouldReturnPaginatedResult()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        List<VideoEntity> videos = VideoFactory.CreateManyWithCategory(CategoryId, category, 3);
        var query = new PublicGetPublishedVideosQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            CategoryId: null,
            TagSlug: null
        );

        _videoRepositoryMock.SetupGetAllAsync(videos, videos.Count);

        // Act
        PublicGetPublishedVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Items.Should().HaveCount(videos.Count);
        result.Videos.Count.Should().Be((long)videos.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPublishedVideosExist_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var query = new PublicGetPublishedVideosQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            CategoryId: null,
            TagSlug: null
        );

        _videoRepositoryMock.SetupGetAllAsync(new List<VideoEntity>(), 0);

        // Act
        PublicGetPublishedVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Items.Should().BeEmpty();
        result.Videos.Count.Should().Be(0);
    }
}
