using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
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
    /// Builds a playlist link with the <c>Video</c> navigation populated via reflection,
    /// so the mapper can read the video's title and rating without a database.
    /// </summary>
    private static PlaylistVideoEntity LinkVideo(Guid playlistId, VideoEntity video, int sortOrder)
    {
        PlaylistVideoEntity link = PlaylistVideoEntity.Create(Guid.NewGuid(), playlistId, video.Id, sortOrder);
        link.GetType().GetProperty("Video")!.SetValue(link, video);
        return link;
    }

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
}
