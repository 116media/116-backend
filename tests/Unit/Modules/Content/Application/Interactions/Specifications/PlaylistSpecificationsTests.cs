using _116.Content.Application.Interactions.Specifications;
using _116.Content.Domain.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.Specifications;

/// <summary>
/// Unit tests for playlist specification classes.
/// </summary>
public class PlaylistSpecificationsTests
{
    #region PlaylistByIdSpecification

    [Fact]
    public void PlaylistByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        Guid id = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistEntity.Create(id, userId: Guid.NewGuid(), name: "My Playlist");
        var spec = new PlaylistByIdSpecification(id);

        bool result = spec.IsSatisfiedBy(playlist);

        result.Should().BeTrue();
    }

    [Fact]
    public void PlaylistByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        PlaylistEntity playlist = PlaylistEntity.Create(Guid.NewGuid(), userId: Guid.NewGuid(), name: "My Playlist");
        var spec = new PlaylistByIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(playlist);

        result.Should().BeFalse();
    }

    #endregion

    #region PlaylistByUserIdSpecification

    [Fact]
    public void PlaylistByUserIdSpecification_WithMatchingUserId_ShouldReturnTrue()
    {
        Guid userId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistEntity.Create(Guid.NewGuid(), userId: userId, name: "My Playlist");
        var spec = new PlaylistByUserIdSpecification(userId);

        bool result = spec.IsSatisfiedBy(playlist);

        result.Should().BeTrue();
    }

    [Fact]
    public void PlaylistByUserIdSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        PlaylistEntity playlist = PlaylistEntity.Create(Guid.NewGuid(), userId: Guid.NewGuid(), name: "My Playlist");
        var spec = new PlaylistByUserIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(playlist);

        result.Should().BeFalse();
    }

    #endregion

    #region PlaylistVideoByPlaylistAndVideoSpecification

    [Fact]
    public void PlaylistVideoByPlaylistAndVideoSpecification_WithMatchingPlaylistAndVideo_ShouldReturnTrue()
    {
        Guid playlistId = Guid.NewGuid();
        Guid videoId = Guid.NewGuid();
        PlaylistVideoEntity entry = PlaylistVideoEntity.Create(
            Guid.NewGuid(),
            playlistId: playlistId,
            videoId: videoId,
            sortOrder: 1
        );
        var spec = new PlaylistVideoByPlaylistAndVideoSpecification(playlistId, videoId);

        bool result = spec.IsSatisfiedBy(entry);

        result.Should().BeTrue();
    }

    [Fact]
    public void PlaylistVideoByPlaylistAndVideoSpecification_WithDifferentPlaylistId_ShouldReturnFalse()
    {
        PlaylistVideoEntity entry = PlaylistVideoEntity.Create(
            Guid.NewGuid(),
            playlistId: Guid.NewGuid(),
            videoId: Guid.NewGuid(),
            sortOrder: 1
        );
        var spec = new PlaylistVideoByPlaylistAndVideoSpecification(Guid.NewGuid(), entry.VideoId);

        bool result = spec.IsSatisfiedBy(entry);

        result.Should().BeFalse();
    }

    [Fact]
    public void PlaylistVideoByPlaylistAndVideoSpecification_WithDifferentVideoId_ShouldReturnFalse()
    {
        Guid playlistId = Guid.NewGuid();
        PlaylistVideoEntity entry = PlaylistVideoEntity.Create(
            Guid.NewGuid(),
            playlistId: playlistId,
            videoId: Guid.NewGuid(),
            sortOrder: 1
        );
        var spec = new PlaylistVideoByPlaylistAndVideoSpecification(playlistId, Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(entry);

        result.Should().BeFalse();
    }

    #endregion
}
