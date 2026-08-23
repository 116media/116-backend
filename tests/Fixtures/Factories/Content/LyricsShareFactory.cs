using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="LyricsShareEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class LyricsShareFactory
{
    /// <summary>
    /// Creates an anonymous share record (no user, no channel) for the given lyrics page.
    /// </summary>
    public static LyricsShareEntity CreateAnonymous(Guid lyricsId) =>
        LyricsShareEntity.Create(Guid.NewGuid(), null, lyricsId);
}
