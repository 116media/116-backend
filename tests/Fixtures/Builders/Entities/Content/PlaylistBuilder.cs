using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PlaylistEntity"/> instances in tests.
/// For test code, prefer using PlaylistFactory instead of direct Builder usage.
/// </summary>
internal class PlaylistBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();
    private string _name = TestConstants.Content.Interactions.ValidPlaylistName;

    /// <summary>Sets the playlist ID.</summary>
    public PlaylistBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the user ID (owner).</summary>
    public PlaylistBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>Sets the playlist name.</summary>
    public PlaylistBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>Builds the <see cref="PlaylistEntity"/> instance.</summary>
    public PlaylistEntity Build() => PlaylistEntity.Create(id: _id, userId: _userId, name: _name);
}
