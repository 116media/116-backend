using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="PlaylistBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class PlaylistFactory
{
    /// <summary>
    /// Creates a playlist owned by the given user.
    /// </summary>
    public static PlaylistEntity Create(Guid userId) => new PlaylistBuilder().WithUserId(userId).Build();

    /// <summary>
    /// Creates a playlist with a specific ID owned by the given user.
    /// </summary>
    public static PlaylistEntity CreateWithId(Guid id, Guid userId) =>
        new PlaylistBuilder().WithId(id).WithUserId(userId).Build();

    /// <summary>
    /// Creates a list of playlists owned by the given user.
    /// </summary>
    public static IReadOnlyList<PlaylistEntity> CreateMany(int count, Guid userId) =>
        Enumerable.Range(0, count).Select(_ => Create(userId)).ToList();
}
