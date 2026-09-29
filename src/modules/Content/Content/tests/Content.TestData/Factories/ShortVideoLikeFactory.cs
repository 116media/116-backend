using _116.Content.Domain.Entities;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="ShortVideoLikeEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ShortVideoLikeFactory
{
    /// <summary>
    /// Creates a like record for the given user and short video.
    /// </summary>
    public static ShortVideoLikeEntity Create(Guid userId, Guid shortVideoId) =>
        ShortVideoLikeEntity.Create(Guid.NewGuid(), userId, shortVideoId);
}
