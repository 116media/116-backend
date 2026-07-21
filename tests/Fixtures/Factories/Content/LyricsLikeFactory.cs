using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="LyricsLikeEntity"/> instances in tests.
/// </summary>
public static class LyricsLikeFactory
{
    /// <summary>
    /// Creates a like record for the given user and lyrics page.
    /// </summary>
    public static LyricsLikeEntity Create(Guid userId, Guid lyricsId) =>
        LyricsLikeEntity.Create(Guid.NewGuid(), userId, lyricsId);

    /// <summary>
    /// Creates a like record for a random user against the given lyrics page.
    /// </summary>
    public static LyricsLikeEntity CreateForLyrics(Guid lyricsId) => Create(Guid.NewGuid(), lyricsId);
}
