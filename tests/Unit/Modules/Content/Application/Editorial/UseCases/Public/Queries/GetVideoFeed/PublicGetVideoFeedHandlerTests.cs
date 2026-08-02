using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Unit tests for <see cref="PublicGetVideoFeedHandler"/>.
/// </summary>
public class PublicGetVideoFeedHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetVideoFeedHandler _handler;

    public PublicGetVideoFeedHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new PublicGetVideoFeedHandler(
            _categoryRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenNoPinnedCategories_ShouldReturnEmptySections()
    {
        PublicGetVideoFeedResult result = await _handler.Handle(new PublicGetVideoFeedQuery(), CancellationToken.None);

        result.Sections.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOmitEmptySections_AndBatchFilesOnce()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity withVideos = CategoryFactory.CreatePinned(videoType);
        CategoryEntity empty = CategoryFactory.CreatePinned(videoType);

        List<VideoEntity> videos = VideoFactory.CreateManyWithCategory(withVideos.Id, withVideos, 3);

        _categoryRepositoryMock.SetupGetPinnedToFeedCategories([withVideos, empty]);
        _videoRepositoryMock.SetupGetLatestPublishedByCategory(withVideos.Id, videos);
        _videoRepositoryMock.SetupGetLatestPublishedByCategory(empty.Id, new List<VideoEntity>());

        PublicGetVideoFeedResult result = await _handler.Handle(new PublicGetVideoFeedQuery(), CancellationToken.None);

        result.Sections.Should().ContainSingle();
        result.Sections[0].Category.Id.Should().Be(withVideos.Id);
        result.Sections[0].Videos.Should().HaveCount(3);
        _fileRepositoryMock.VerifyGetByIdsCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldExcludeNonVideoPinnedCategories()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        ContentTypeEntity articleType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Article));
        CategoryEntity video = CategoryFactory.CreatePinned(videoType);
        CategoryEntity article = CategoryFactory.CreatePinned(articleType);

        _categoryRepositoryMock.SetupGetPinnedToFeedCategories([video, article]);
        _videoRepositoryMock.SetupGetLatestPublishedByCategory(
            video.Id,
            VideoFactory.CreateManyWithCategory(video.Id, video, 2)
        );

        PublicGetVideoFeedResult result = await _handler.Handle(new PublicGetVideoFeedQuery(), CancellationToken.None);

        result.Sections.Should().ContainSingle(s => s.Category.Id == video.Id);
    }

    [Fact]
    public async Task Handle_ShouldRequestMaxVideosPerSection()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.CreatePinned(videoType);

        _categoryRepositoryMock.SetupGetPinnedToFeedCategories([category]);
        _videoRepositoryMock.SetupGetLatestPublishedByCategory(
            category.Id,
            VideoFactory.CreateManyWithCategory(category.Id, category, 1)
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
