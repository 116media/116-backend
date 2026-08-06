using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="LyricsLikeEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class LyricsLikeFactory
{
    /// <summary>
    /// Creates a like record for the given user and lyrics page.
    /// </summary>
    public static LyricsLikeEntity Create(Guid userId, Guid lyricsId) =>
        LyricsLikeEntity.Create(Guid.NewGuid(), userId, lyricsId);
}
