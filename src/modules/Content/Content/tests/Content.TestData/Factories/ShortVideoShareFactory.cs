using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="ShortVideoShareEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ShortVideoShareFactory
{
    /// <summary>
    /// Creates a share record attributed to the given user.
    /// </summary>
    public static ShortVideoShareEntity Create(Guid userId, Guid shortVideoId, EnumShareChannel? shareChannel = null) =>
        ShortVideoShareEntity.Create(Guid.NewGuid(), userId, shortVideoId, shareChannel);

    /// <summary>
    /// Creates a share record with no signed-in user behind it.
    /// </summary>
    public static ShortVideoShareEntity CreateAnonymous(Guid shortVideoId, EnumShareChannel? shareChannel = null) =>
        ShortVideoShareEntity.Create(Guid.NewGuid(), null, shortVideoId, shareChannel);
}
