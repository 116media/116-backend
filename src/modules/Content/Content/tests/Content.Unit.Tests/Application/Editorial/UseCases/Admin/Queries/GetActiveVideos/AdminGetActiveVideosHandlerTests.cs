using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos;
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

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos;

/// <summary>
/// Unit tests for <see cref="AdminGetActiveVideosHandler"/>.
/// </summary>
public class AdminGetActiveVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminGetActiveVideosHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetActiveVideosHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminGetActiveVideosHandler(
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
    public async Task Handle_WhenActiveVideosExist_ShouldReturnAllActiveVideos()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        List<VideoEntity> videos = VideoFactory.CreateManyWithCategory(category, 3);
        var query = new AdminGetActiveVideosQuery();

        _videoRepositoryMock.SetupGetActiveAsync(videos);

        // Act
        AdminGetActiveVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WhenNoActiveVideosExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new AdminGetActiveVideosQuery();

        _videoRepositoryMock.SetupGetActiveAsync(new List<VideoEntity>());

        // Act
        AdminGetActiveVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryGetActiveAsync()
    {
        // Arrange
        var query = new AdminGetActiveVideosQuery();

        _videoRepositoryMock.SetupGetActiveAsync(new List<VideoEntity>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _videoRepositoryMock.Verify(x => x.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
