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

    /// <summary>
    /// Sets up the lookup to return the playlist only for its own id, so a handler that looks up a
    /// different playlist is not silently satisfied.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="playlist">The playlist returned for its own identifier.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IPlaylistRepository> SetupGetByIdAsync(
        this Mock<IPlaylistRepository> mock,
        PlaylistEntity playlist
    )
    {
        Guid playlistId = playlist.Id;
        mock.Setup(x => x.GetByIdAsync(It.Is<Guid>(id => id == playlistId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(playlist);
        return mock;
    }

    /// <summary>
    /// Arranges a miss for <paramref name="playlistId" />. Naming the identifier is what separates
    /// "this playlist does not exist" from "no lookup this handler makes can succeed".
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="playlistId">The identifier that must resolve to nothing.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IPlaylistRepository> SetupGetByIdNotFound(this Mock<IPlaylistRepository> mock, Guid playlistId)
    {
        mock.Setup(x => x.GetByIdAsync(playlistId, It.IsAny<CancellationToken>())).ReturnsAsync((PlaylistEntity?)null);
        return mock;
    }

    /// <summary>
    /// Sets up the videos-included lookup to return the playlist only for its own id.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="playlist">The playlist returned for its own identifier.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IPlaylistRepository> SetupGetByIdWithVideosAsync(
        this Mock<IPlaylistRepository> mock,
        PlaylistEntity playlist
    )
    {
        Guid playlistId = playlist.Id;
        mock.Setup(x => x.GetByIdWithVideosAsync(It.Is<Guid>(id => id == playlistId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(playlist);
        return mock;
    }

    /// <summary>
    /// Arranges a miss for <paramref name="playlistId" /> on the videos-included lookup.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="playlistId">The identifier that must resolve to nothing.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IPlaylistRepository> SetupGetByIdWithVideosNotFound(
        this Mock<IPlaylistRepository> mock,
        Guid playlistId
    )
    {
        mock.Setup(x => x.GetByIdWithVideosAsync(playlistId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlaylistEntity?)null);
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

    /// <summary>
    /// Verifies that the repository was handed exactly the expected entity once,
    /// so updating a different instance than the one looked up fails the test.
    /// </summary>
    public static void VerifyUpdateCalled(this Mock<IPlaylistRepository> mock, PlaylistEntity expected)
    {
        mock.Verify(x => x.Update(expected), Times.Once);
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

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<IPlaylistRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<PlaylistEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddVideoAsync(It.IsAny<PlaylistVideoEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.RemoveVideoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlaylistEntity>());
        mock.Setup(x => x.VideoExistsInPlaylistAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }
}
