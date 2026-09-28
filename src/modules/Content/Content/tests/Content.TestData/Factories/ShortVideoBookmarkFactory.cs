using _116.Content.Domain.Entities;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="ShortVideoBookmarkEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ShortVideoBookmarkFactory
{
    /// <summary>
    /// Creates a bookmark record for the given user and short video.
    /// </summary>
    public static ShortVideoBookmarkEntity Create(Guid userId, Guid shortVideoId) =>
        ShortVideoBookmarkEntity.Create(Guid.NewGuid(), userId, shortVideoId);
}
