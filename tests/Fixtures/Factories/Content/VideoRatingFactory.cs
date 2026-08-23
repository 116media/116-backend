using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="VideoRatingEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class VideoRatingFactory
{
    /// <summary>
    /// Creates a video rating with the given parameters.
    /// </summary>
    public static VideoRatingEntity Create(Guid videoId, Guid userId, short stars = 5) =>
        VideoRatingEntity.Create(id: Guid.NewGuid(), userId: userId, videoId: videoId, stars: stars);
}
