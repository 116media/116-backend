using _116.Content.Domain.Entities;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="LyricsTagEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class LyricsTagFactory
{
    /// <summary>
    /// Creates a tag link between the given lyrics page and tag.
    /// </summary>
    public static LyricsTagEntity Create(Guid lyricsId, Guid tagId) =>
        LyricsTagEntity.Create(Guid.NewGuid(), lyricsId, tagId);
}
