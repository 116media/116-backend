using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;
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

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Unit tests for <see cref="PublicGetVideoFeedHandler"/>.
/// </summary>
public class PublicGetVideoFeedHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetVideoFeedHandler _handler;

    public PublicGetVideoFeedHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _contentTypeRepositoryMock = MockContentTypeRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new PublicGetVideoFeedHandler(
            new PublicVideoFeedService(
                _categoryRepositoryMock.Object,
                _contentTypeRepositoryMock.Object,
                _videoRepositoryMock.Object
            ),
            CreateCategoryDtoService(_fileStorageMock.Object),
            new VideoDtoService(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupService()
            )
        );
    }

    [Fact]
    public async Task Handle_WhenNoPinnedCategories_ShouldReturnEmptySections()
    {
        PublicGetVideoFeedResult result = await _handler.Handle(new PublicGetVideoFeedQuery(), CancellationToken.None);

        result.Sections.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOmitEmptySections_AndBatchThumbnailsAndPostersSeparately()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        _contentTypeRepositoryMock.SetupGetByIds(videoType);
        CategoryEntity withVideos = CategoryFactory.CreatePinned(videoType);
        CategoryEntity empty = CategoryFactory.CreatePinned(videoType);

        List<VideoEntity> videos = VideoFactory.CreateManyWithCategory(withVideos, 3);

        _categoryRepositoryMock.SetupGetPinnedToFeedCategories([withVideos, empty]);
        _videoRepositoryMock.SetupGetLatestPublishedByCategory(withVideos.Id, videos);
        _videoRepositoryMock.SetupGetLatestPublishedByCategory(empty.Id, new List<VideoEntity>());

        PublicGetVideoFeedResult result = await _handler.Handle(new PublicGetVideoFeedQuery(), CancellationToken.None);

        result.Sections.Should().ContainSingle();
        result.Sections[0].Category.Id.Should().Be(withVideos.Id);
        result.Sections[0].Videos.Should().HaveCount(3);
        // One batch for the video thumbnails, one for the category posters — never per card.
        _fileStorageMock.VerifyResolveManyCalledTimes(2);
    }

    [Fact]
    public async Task Handle_ShouldExcludeNonVideoPinnedCategories()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        ContentTypeEntity articleType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Article));
        _contentTypeRepositoryMock.SetupGetByIds(videoType, articleType);
        CategoryEntity video = CategoryFactory.CreatePinned(videoType);
        CategoryEntity article = CategoryFactory.CreatePinned(articleType);

        _categoryRepositoryMock.SetupGetPinnedToFeedCategories([video, article]);
        _videoRepositoryMock.SetupGetLatestPublishedByCategory(video.Id, VideoFactory.CreateManyWithCategory(video, 2));

        PublicGetVideoFeedResult result = await _handler.Handle(new PublicGetVideoFeedQuery(), CancellationToken.None);

        result.Sections.Should().ContainSingle(s => s.Category.Id == video.Id);
    }

    [Fact]
    public async Task Handle_ShouldRequestMaxVideosPerSection()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        _contentTypeRepositoryMock.SetupGetByIds(videoType);
        CategoryEntity category = CategoryFactory.CreatePinned(videoType);

        _categoryRepositoryMock.SetupGetPinnedToFeedCategories([category]);
        _videoRepositoryMock.SetupGetLatestPublishedByCategory(
            category.Id,
            VideoFactory.CreateManyWithCategory(category, 1)
        );

        await _handler.Handle(new PublicGetVideoFeedQuery(), CancellationToken.None);

        _videoRepositoryMock.Verify(
            x =>
                x.GetLatestPublishedByCategoryAsync(
                    category.Id,
                    EditorialFeedConstants.MaxVideosPerFeedSection,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
