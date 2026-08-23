using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="ShortVideoBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ShortVideoFactory
{
    /// <summary>
    /// Creates a standalone active short video.
    /// </summary>
    public static ShortVideoEntity Create() => new ShortVideoBuilder().Build();

    /// <summary>
    /// Creates a teaser short video linked to a parent full video.
    /// </summary>
    public static ShortVideoEntity CreateTeaser(Guid videoId) => new ShortVideoBuilder().AsTeaser(videoId).Build();

    /// <summary>
    /// Creates an inactive standalone short video.
    /// </summary>
    public static ShortVideoEntity CreateInactive() => new ShortVideoBuilder().AsInactive().Build();

    /// <summary>
    /// Creates a file-less inactive draft short video, simulating a short video created before its
    /// video file has been uploaded.
    /// </summary>
    public static ShortVideoEntity CreateDraft() => new ShortVideoBuilder().WithoutVideoFile().AsInactive().Build();

    /// <summary>
    /// Creates a short video with a known slug.
    /// </summary>
    public static ShortVideoEntity CreateWithSlug(string slug) => new ShortVideoBuilder().WithSlug(slug).Build();

    /// <summary>
    /// Creates a standalone active short video authored by a specific user.
    /// </summary>
    public static ShortVideoEntity CreateWithAuthorId(Guid authorId) =>
        new ShortVideoBuilder().WithAuthorId(authorId).Build();

    /// <summary>
    /// Creates a short video with a thumbnail file ID attached.
    /// </summary>
    public static ShortVideoEntity CreateWithThumbnail() => new ShortVideoBuilder().WithThumbnail().Build();

    /// <summary>
    /// Creates a list of standalone active short videos.
    /// </summary>
    public static List<ShortVideoEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
