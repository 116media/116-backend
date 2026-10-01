using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;
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

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;

/// <summary>
/// Unit tests for <see cref="PublicGetPromotedVideosHandler"/>.
/// </summary>
public class PublicGetPromotedVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetPromotedVideosHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPromotedVideosHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new PublicGetPromotedVideosHandler(
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
    public async Task Handle_WhenPromotedVideosExist_ShouldReturnVideoList()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        List<VideoEntity> promoted = VideoFactory.CreateManyWithCategory(category, 2);
        var query = new PublicGetPromotedVideosQuery();

        _videoRepositoryMock.SetupGetPromotedAsync(promoted);

        // Act
        PublicGetPromotedVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Count.Should().Be(promoted.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPromotedVideosExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new PublicGetPromotedVideosQuery();

        _videoRepositoryMock.SetupGetPromotedAsync(new List<VideoEntity>());

        // Act
        PublicGetPromotedVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Should().BeEmpty();
    }
}
