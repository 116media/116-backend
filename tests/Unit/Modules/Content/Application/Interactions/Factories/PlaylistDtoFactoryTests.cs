using _116.Content.Application.Interactions.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.Services;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.Factories;

/// <summary>
/// Unit tests for <see cref="PlaylistDtoFactory"/>, covering the <see cref="VideoInPlaylistDto"/>
/// projection (field mapping, sort ordering, and batched thumbnail resolution).
/// </summary>
public class PlaylistDtoFactoryTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly Mock<IVideoRepository> _videoRepositoryMock = MockVideoRepository.Create();

    /// <summary>
    /// Builds the factory over the shared mapper, the mocked storage contract and the mocked
    /// published-video lookup the projection resolves its entries through.
    /// </summary>
    /// <returns>The factory.</returns>
    private PlaylistDtoFactory CreateFactory() =>
        new(Mapper, _fileStorageMock.Object, _videoRepositoryMock.Object, MockCategoryRepository.Create().Object);

    /// <summary>
    /// Links a video to the playlist and arranges the published-video lookup to resolve it, as
    /// the repository would for a published entry.
    /// </summary>
    private PlaylistVideoEntity LinkVideo(PlaylistEntity playlist, VideoEntity video, int sortOrder)
    {
        PlaylistVideoEntity link = new PlaylistVideoBuilder(playlist).WithVideo(video).WithSortOrder(sortOrder).Build();
        _videoRepositoryMock.SetupGetByIds([.. playlist.Videos.Select(Resolve).OfType<VideoEntity>(), video]);

        return link;
    }

    /// <summary>
    /// The video a link points at, when a previous link in this test already arranged it.
    /// </summary>
    private VideoEntity? Resolve(PlaylistVideoEntity link) =>
        _videoRepositoryMock.Object.GetByIdsAsync([link.VideoId]).Result.GetValueOrDefault(link.VideoId);

    [Fact]
    public async Task CreateDetailAsync_ShouldMapPlaylistIdAndName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.Create(userId);

        // Act
        PlaylistDetailDto dto = await CreateFactory().CreateDetailAsync(playlist, CancellationToken.None);

        // Assert
        dto.Id.Should().Be(playlist.Id);
        dto.Name.Should().Be(playlist.Name);
    }

    [Fact]
    public async Task CreateDetailAsync_ShouldMapVideoFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.WithRating(average: 4.5m, count: 20);
        video.WithShareCount(1);

        PlaylistEntity playlist = PlaylistFactory.Create(userId);
        LinkVideo(playlist, video, sortOrder: 1);

        // Act
        PlaylistDetailDto dto = await CreateFactory().CreateDetailAsync(playlist, CancellationToken.None);

        // Assert
        dto.Videos.Should().ContainSingle();
        VideoInPlaylistDto mapped = dto.Videos[0];
        mapped.VideoId.Should().Be(video.Id);
        mapped.Title.Should().Be(video.Title);
        mapped.Slug.Should().Be(video.Slug);
        mapped.CategoryName.Should().BeEmpty();
        mapped.PublishedAt.Should().Be(video.PublishedAt);
        mapped.ShareCount.Should().Be(1);
        mapped.RatingAverage.Should().Be(4.5m);
        mapped.RatingCount.Should().Be(20);
        mapped.SortOrder.Should().Be(1);
        mapped.ThumbnailUrl.Should().BeNull();
    }

    [Fact]
    public async Task CreateDetailAsync_ShouldOrderVideosBySortOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        VideoEntity first = VideoFactory.Create(CategoryId);
        VideoEntity second = VideoFactory.Create(CategoryId);

        PlaylistEntity playlist = PlaylistFactory.Create(userId);
        LinkVideo(playlist, first, sortOrder: 2);
        LinkVideo(playlist, second, sortOrder: 1);

        // Act
        PlaylistDetailDto dto = await CreateFactory().CreateDetailAsync(playlist, CancellationToken.None);

        // Assert — the sort-order-1 video comes first
        dto.Videos.Should().HaveCount(2);
        dto.Videos[0].VideoId.Should().Be(second.Id);
        dto.Videos[1].VideoId.Should().Be(first.Id);
    }

    [Fact]
    public async Task CreateDetailAsync_WhenNoVideos_ShouldReturnEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.Create(userId);

        // Act
        PlaylistDetailDto dto = await CreateFactory().CreateDetailAsync(playlist, CancellationToken.None);

        // Assert
        dto.Videos.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task CreateManyAsync_ReturnsFirstFourOrderedNullableThumbnailSlots(int videoCount)
    {
        PlaylistEntity playlist = PlaylistFactory.Create(Guid.NewGuid());
        var resolvedUrls = new Dictionary<Guid, string>();
        for (int index = 0; index < videoCount; index++)
        {
            VideoEntity video =
                index % 2 == 0 ? VideoFactory.CreateWithThumbnail(CategoryId) : VideoFactory.Create(CategoryId);
            LinkVideo(playlist, video, sortOrder: index);
            if (video.ThumbnailFileId is { } thumbnailFileId)
            {
                resolvedUrls[thumbnailFileId] = $"https://cdn.example/{index}.jpg";
            }
        }
        _fileStorageMock
            .Setup(repository =>
                repository.ResolveUrlsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(resolvedUrls);

        IReadOnlyList<PlaylistDto> result = await CreateFactory().CreateManyAsync([playlist], CancellationToken.None);

        PlaylistDto dto = result.Should().ContainSingle().Subject;
        dto.VideoCount.Should().Be(videoCount);
        dto.ThumbnailUrls.Should().HaveCount(Math.Min(videoCount, 4));
        for (int index = 0; index < Math.Min(videoCount, 4); index++)
        {
            dto.ThumbnailUrls[index].Should().Be(index % 2 == 0 ? $"https://cdn.example/{index}.jpg" : null);
        }
        if (videoCount > 0)
        {
            _fileStorageMock.Verify(
                repository =>
                    repository.ResolveUrlsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }

    [Fact]
    public async Task CreateDetailAsync_ResolvesAllThumbnailsInOneBatch()
    {
        PlaylistEntity playlist = PlaylistFactory.Create(Guid.NewGuid());
        VideoEntity first = VideoFactory.CreateWithThumbnail(CategoryId);
        VideoEntity second = VideoFactory.CreateWithThumbnail(CategoryId);
        LinkVideo(playlist, first, 0);
        LinkVideo(playlist, second, 1);
        var urls = new Dictionary<Guid, string>
        {
            [first.ThumbnailFileId!.Value] = "https://cdn.example/first.jpg",
            [second.ThumbnailFileId!.Value] = "https://cdn.example/second.jpg",
        };
        _fileStorageMock
            .Setup(repository =>
                repository.ResolveUrlsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(urls);

        PlaylistDetailDto dto = await CreateFactory().CreateDetailAsync(playlist, CancellationToken.None);

        dto.Videos.Select(video => video.ThumbnailUrl)
            .Should()
            .Equal("https://cdn.example/first.jpg", "https://cdn.example/second.jpg");
        _fileStorageMock.Verify(
            repository =>
                repository.ResolveUrlsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _fileStorageMock.Verify(
            repository => repository.ResolveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
