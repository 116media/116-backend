using _116.Content.Domain.Entities;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="VideoTagEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class VideoTagFactory
{
    /// <summary>
    /// Creates a tag link between the given video and tag.
    /// </summary>
    public static VideoTagEntity Create(Guid videoId, Guid tagId) =>
        VideoTagEntity.Create(Guid.NewGuid(), videoId, tagId);
}
