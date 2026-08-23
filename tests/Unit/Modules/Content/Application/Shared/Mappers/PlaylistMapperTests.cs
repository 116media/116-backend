using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="PlaylistMapper"/>, covering the <see cref="VideoInPlaylistDto"/>
/// projection (field mapping, sort ordering, and thumbnail resolution).
/// </summary>
public class PlaylistMapperTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private readonly Mock<IFileRepository> _fileRepositoryMock = new();

    /// <summary>
    /// Builds a playlist link carrying the Video navigation EF Core would populate, so the mapper
    /// can read the video's title and rating without a database.
    /// </summary>
    private static PlaylistVideoEntity LinkVideo(Guid playlistId, VideoEntity video, int sortOrder) =>
        new PlaylistVideoBuilder().WithPlaylistId(playlistId).WithVideo(video).WithSortOrder(sortOrder).Build();

    [Fact]
    public async Task ToPlaylistDetailDtoAsync_ShouldMapPlaylistIdAndName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.Create(userId);

        // Act
        PlaylistDetailDto dto = await playlist.ToPlaylistDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.Id.Should().Be(playlist.Id);
        dto.Name.Should().Be(playlist.Name);
    }

    [Fact]
    public async Task ToPlaylistDetailDtoAsync_ShouldMapVideoFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.UpdateRating(average: 4.5m, count: 20);
        video.IncrementShareCount();

        PlaylistEntity playlist = PlaylistFactory.Create(userId);
        playlist.Videos.Add(LinkVideo(playlist.Id, video, sortOrder: 1));

        // Act
        PlaylistDetailDto dto = await playlist.ToPlaylistDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

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
    public async Task ToPlaylistDetailDtoAsync_ShouldOrderVideosBySortOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        VideoEntity first = VideoFactory.Create(CategoryId);
        VideoEntity second = VideoFactory.Create(CategoryId);

        PlaylistEntity playlist = PlaylistFactory.Create(userId);
        playlist.Videos.Add(LinkVideo(playlist.Id, first, sortOrder: 2));
        playlist.Videos.Add(LinkVideo(playlist.Id, second, sortOrder: 1));

        // Act
        PlaylistDetailDto dto = await playlist.ToPlaylistDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert — the sort-order-1 video comes first
        dto.Videos.Should().HaveCount(2);
        dto.Videos[0].VideoId.Should().Be(second.Id);
        dto.Videos[1].VideoId.Should().Be(first.Id);
    }

    [Fact]
    public async Task ToPlaylistDetailDtoAsync_WhenNoVideos_ShouldReturnEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.Create(userId);

        // Act
        PlaylistDetailDto dto = await playlist.ToPlaylistDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

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
    public async Task ToPlaylistDtosAsync_ReturnsFirstFourOrderedNullableThumbnailSlots(int videoCount)
    {
        PlaylistEntity playlist = PlaylistFactory.Create(Guid.NewGuid());
        var resolvedUrls = new Dictionary<Guid, string>();
        for (int index = 0; index < videoCount; index++)
        {
            VideoEntity video =
                index % 2 == 0 ? VideoFactory.CreateWithThumbnail(CategoryId) : VideoFactory.Create(CategoryId);
            playlist.Videos.Add(LinkVideo(playlist.Id, video, sortOrder: index));
            if (video.ThumbnailFileId is { } thumbnailFileId)
            {
                resolvedUrls[thumbnailFileId] = $"https://cdn.example/{index}.jpg";
            }
        }
        _fileRepositoryMock
            .Setup(repository =>
                repository.GetStorageUrlsByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(resolvedUrls);

        IReadOnlyList<PlaylistDto> result = await new[] { playlist }.ToPlaylistDtosAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        PlaylistDto dto = result.Should().ContainSingle().Subject;
        dto.VideoCount.Should().Be(videoCount);
        dto.ThumbnailUrls.Should().HaveCount(Math.Min(videoCount, 4));
        for (int index = 0; index < Math.Min(videoCount, 4); index++)
        {
            dto.ThumbnailUrls[index].Should().Be(index % 2 == 0 ? $"https://cdn.example/{index}.jpg" : null);
        }
        if (videoCount > 0)
        {
            _fileRepositoryMock.Verify(
                repository =>
                    repository.GetStorageUrlsByIdsAsync(
                        It.IsAny<IReadOnlyCollection<Guid>>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }

    [Fact]
    public async Task ToPlaylistDetailDtoAsync_ResolvesAllThumbnailsInOneBatch()
    {
        PlaylistEntity playlist = PlaylistFactory.Create(Guid.NewGuid());
        VideoEntity first = VideoFactory.CreateWithThumbnail(CategoryId);
        VideoEntity second = VideoFactory.CreateWithThumbnail(CategoryId);
        playlist.Videos.Add(LinkVideo(playlist.Id, first, 0));
        playlist.Videos.Add(LinkVideo(playlist.Id, second, 1));
        var urls = new Dictionary<Guid, string>
        {
            [first.ThumbnailFileId!.Value] = "https://cdn.example/first.jpg",
            [second.ThumbnailFileId!.Value] = "https://cdn.example/second.jpg",
        };
        _fileRepositoryMock
            .Setup(repository =>
                repository.GetStorageUrlsByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(urls);

        PlaylistDetailDto dto = await playlist.ToPlaylistDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        dto.Videos.Select(video => video.ThumbnailUrl)
            .Should()
            .Equal("https://cdn.example/first.jpg", "https://cdn.example/second.jpg");
        _fileRepositoryMock.Verify(
            repository =>
                repository.GetStorageUrlsByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _fileRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
