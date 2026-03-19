using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="PlaylistEntity"/> instances in tests.
/// </summary>
public static class PlaylistFactory
{
    /// <summary>Creates a playlist owned by the given user.</summary>
    public static PlaylistEntity Create(Guid userId) => new PlaylistBuilder().WithUserId(userId).Build();

    /// <summary>Creates a playlist with a specific ID owned by the given user.</summary>
    public static PlaylistEntity CreateWithId(Guid id, Guid userId) =>
        new PlaylistBuilder().WithId(id).WithUserId(userId).Build();

    /// <summary>Creates a playlist with videos already added to its collection.</summary>
    public static PlaylistEntity CreateWithVideos(Guid userId, IReadOnlyList<PlaylistVideoEntity> videos)
    {
        PlaylistEntity playlist = Create(userId);
        foreach (PlaylistVideoEntity video in videos)
        {
            playlist.Videos.Add(video);
        }

        return playlist;
    }

    /// <summary>Creates a list of playlists owned by the given user.</summary>
    public static IReadOnlyList<PlaylistEntity> CreateMany(int count, Guid userId) =>
        Enumerable.Range(0, count).Select(_ => Create(userId)).ToList();
}
