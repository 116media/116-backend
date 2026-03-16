using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="ShortVideoEntity"/> instances in tests.
/// </summary>
public static class ShortVideoFactory
{
    /// <summary>Creates a standalone active short video.</summary>
    public static ShortVideoEntity Create() => new ShortVideoBuilder().Build();

    /// <summary>Creates a standalone short video with a specific ID.</summary>
    public static ShortVideoEntity CreateWithId(Guid id) => new ShortVideoBuilder().WithId(id).Build();

    /// <summary>Creates a teaser short video linked to a parent full video.</summary>
    public static ShortVideoEntity CreateTeaser(Guid videoId) => new ShortVideoBuilder().AsTeaser(videoId).Build();

    /// <summary>Creates an inactive standalone short video.</summary>
    public static ShortVideoEntity CreateInactive() => new ShortVideoBuilder().AsInactive().Build();

    /// <summary>Creates a short video with a known slug.</summary>
    public static ShortVideoEntity CreateWithSlug(string slug) => new ShortVideoBuilder().WithSlug(slug).Build();

    /// <summary>Creates a short video with a thumbnail attached.</summary>
    public static ShortVideoEntity CreateWithThumbnail(
        string thumbnailUrl = TestConstants.Content.Editorial.ArticleImage.ValidUrl,
        string thumbnailStorageKey = TestConstants.Content.Editorial.ArticleImage.ValidStorageKey
    ) => new ShortVideoBuilder().WithThumbnail(thumbnailUrl, thumbnailStorageKey).Build();

    /// <summary>Creates a list of standalone active short videos.</summary>
    public static List<ShortVideoEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();

    /// <summary>Creates a short video with the default known values from TestConstants.</summary>
    public static ShortVideoEntity CreateDefault() =>
        new ShortVideoBuilder()
            .WithTitle(TestConstants.Content.Editorial.ShortVideo.ValidTitle)
            .WithSlug(TestConstants.Content.Editorial.ShortVideo.ValidSlug)
            .Build();
}
