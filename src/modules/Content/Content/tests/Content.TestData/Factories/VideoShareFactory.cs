using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="VideoShareEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class VideoShareFactory
{
    /// <summary>
    /// Creates a share record attributed to the given user.
    /// </summary>
    public static VideoShareEntity Create(Guid userId, Guid videoId, EnumShareChannel? shareChannel = null) =>
        VideoShareEntity.Create(Guid.NewGuid(), userId, videoId, shareChannel);

    /// <summary>
    /// Creates a share record with no signed-in user behind it.
    /// </summary>
    public static VideoShareEntity CreateAnonymous(Guid videoId, EnumShareChannel? shareChannel = null) =>
        VideoShareEntity.Create(Guid.NewGuid(), null, videoId, shareChannel);
}
