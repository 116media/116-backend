using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Unit tests for <see cref="PublicVideoFeedService"/>: the video-only filter over the pinned
/// categories and the per-section video load.
/// </summary>
public class PublicVideoFeedServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock = MockContentTypeRepository.Create();
    private readonly Mock<IVideoRepository> _videoRepositoryMock = MockVideoRepository.Create();
    private readonly PublicVideoFeedService _service;

    public PublicVideoFeedServiceTests()
    {
        _service = new PublicVideoFeedService(
            _categoryRepositoryMock.Object,
            _contentTypeRepositoryMock.Object,
            _videoRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetPinnedSectionsAsync_ShouldKeepOnlyVideoCategoriesWithTheirLatestVideos()
    {
        // Arrange
        ContentTypeEntity videoType = ContentTypeFactory.Create("Video");
        ContentTypeEntity articleType = ContentTypeFactory.Create("Article");
        CategoryEntity videoCategory = CategoryFactory.CreatePinned(videoType.Id);
        CategoryEntity articleCategory = CategoryFactory.CreatePinned(articleType.Id);
        VideoEntity video = VideoFactory.CreatePublished(videoCategory.Id);
        _categoryRepositoryMock.SetupGetPinnedToFeedCategories([videoCategory, articleCategory]);
        _contentTypeRepositoryMock.SetupGetByIds(videoType, articleType);
        _videoRepositoryMock.SetupGetLatestPublishedByCategory(videoCategory.Id, [video]);

        // Act
        IReadOnlyList<VideoFeedSection> sections = await _service.GetPinnedSectionsAsync(CancellationToken.None);

        // Assert
        sections.Should().ContainSingle();
        sections[0].Category.Should().BeSameAs(videoCategory);
        sections[0].Videos.Should().ContainSingle(v => v.Id == video.Id);
    }

    [Fact]
    public async Task GetPinnedSectionsAsync_WithNoPinnedVideoCategory_ShouldReturnNothingWithoutLoadingVideos()
    {
        // Arrange
        _categoryRepositoryMock.SetupGetPinnedToFeedCategories([]);
        _contentTypeRepositoryMock.SetupGetByIds();

        // Act
        IReadOnlyList<VideoFeedSection> sections = await _service.GetPinnedSectionsAsync(CancellationToken.None);

        // Assert
        sections.Should().BeEmpty();
        _videoRepositoryMock.Verify(
            x => x.GetLatestPublishedByCategoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
