using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="LyricsShareEntity"/> instances in tests.
/// </summary>
public static class LyricsShareFactory
{
    /// <summary>
    /// Creates an anonymous share record (no user, no channel) for the given lyrics page.
    /// </summary>
    public static LyricsShareEntity CreateAnonymous(Guid lyricsId) =>
        LyricsShareEntity.Create(Guid.NewGuid(), null, lyricsId);

    /// <summary>
    /// Creates an authenticated share record for the given user and lyrics page.
    /// </summary>
    public static LyricsShareEntity CreateForUser(Guid userId, Guid lyricsId) =>
        LyricsShareEntity.Create(Guid.NewGuid(), userId, lyricsId);

    /// <summary>
    /// Creates a share record with an explicit share channel.
    /// </summary>
    public static LyricsShareEntity CreateWithChannel(Guid lyricsId, EnumShareChannel channel, Guid? userId = null) =>
        LyricsShareEntity.Create(Guid.NewGuid(), userId, lyricsId, channel);
}
