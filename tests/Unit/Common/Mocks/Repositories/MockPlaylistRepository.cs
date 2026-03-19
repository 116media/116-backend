using _116.Content.Application.Interactions.Persistence;
using _116.Content.Domain.Entities;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IPlaylistRepository"/>.
/// </summary>
public static class MockPlaylistRepository
{
    /// <summary>
    /// Creates a new mock instance of IPlaylistRepository with safe default setups.
    /// </summary>
    public static Mock<IPlaylistRepository> Create()
    {
        Mock<IPlaylistRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IPlaylistRepository> SetupGetByIdAsync(
        this Mock<IPlaylistRepository> mock,
        PlaylistEntity? playlist
    )
    {
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(playlist);
        return mock;
    }

    public static Mock<IPlaylistRepository> SetupGetByIdWithVideosAsync(
        this Mock<IPlaylistRepository> mock,
        PlaylistEntity? playlist
    )
    {
        mock.Setup(x => x.GetByIdWithVideosAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(playlist);
        return mock;
    }

    public static Mock<IPlaylistRepository> SetupGetByUserIdAsync(
        this Mock<IPlaylistRepository> mock,
        IReadOnlyList<PlaylistEntity> playlists
    )
    {
        mock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(playlists);
        return mock;
    }

    public static Mock<IPlaylistRepository> SetupVideoExistsInPlaylistAsync(
        this Mock<IPlaylistRepository> mock,
        bool result
    )
    {
        mock.Setup(x => x.VideoExistsInPlaylistAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IPlaylistRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<PlaylistEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyUpdateCalled(this Mock<IPlaylistRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<PlaylistEntity>()), Times.Once);
    }

    public static void VerifyDeleteCalled(this Mock<IPlaylistRepository> mock, PlaylistEntity playlist)
    {
        mock.Verify(x => x.Delete(playlist), Times.Once);
    }

    public static void VerifyAddVideoAsyncCalled(this Mock<IPlaylistRepository> mock)
    {
        mock.Verify(x => x.AddVideoAsync(It.IsAny<PlaylistVideoEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyRemoveVideoAsyncCalled(this Mock<IPlaylistRepository> mock, Guid playlistId, Guid videoId)
    {
        mock.Verify(x => x.RemoveVideoAsync(playlistId, videoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetupDefaults(Mock<IPlaylistRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<PlaylistEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddVideoAsync(It.IsAny<PlaylistVideoEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.RemoveVideoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlaylistEntity?)null);
        mock.Setup(x => x.GetByIdWithVideosAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlaylistEntity?)null);
        mock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlaylistEntity>());
        mock.Setup(x => x.VideoExistsInPlaylistAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }
}
