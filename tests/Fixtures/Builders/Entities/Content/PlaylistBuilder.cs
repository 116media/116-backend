using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PlaylistEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; PlaylistFactory only names chains three or more tests share.
/// </summary>
public class PlaylistBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();
    private string _name = TestConstants.Interactions.ValidPlaylistName;

    /// <summary>
    /// Sets the playlist ID.
    /// </summary>
    public PlaylistBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the user ID (owner).
    /// </summary>
    public PlaylistBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PlaylistEntity"/> instance.
    /// </summary>
    public PlaylistEntity Build() => PlaylistEntity.Create(id: _id, userId: _userId, name: _name);
}
