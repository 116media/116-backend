using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="LyricsEntity"/> instances in tests.
/// </summary>
public static class LyricsFactory
{
    /// <summary>Creates a standalone lyrics page.</summary>
    public static LyricsEntity Create() => new LyricsBuilder().Build();

    /// <summary>Creates a standalone lyrics page with a specific ID.</summary>
    public static LyricsEntity CreateWithId(Guid id) => new LyricsBuilder().WithId(id).Build();

    /// <summary>Creates a lyrics page linked to a video.</summary>
    public static LyricsEntity CreateForVideo(Guid videoId) => new LyricsBuilder().ForVideo(videoId).Build();

    /// <summary>Creates a lyrics page linked to an article.</summary>
    public static LyricsEntity CreateForArticle(Guid articleId) => new LyricsBuilder().ForArticle(articleId).Build();

    /// <summary>Creates a lyrics page with specific song title and artist name.</summary>
    public static LyricsEntity Create(string songTitle, string artistName) =>
        new LyricsBuilder().WithSongTitle(songTitle).WithArtistName(artistName).Build();

    /// <summary>Creates a list of standalone lyrics pages.</summary>
    public static List<LyricsEntity> CreateMany(int count) => Enumerable.Range(0, count).Select(_ => Create()).ToList();

    /// <summary>Creates a lyrics page with the default known values from TestConstants.</summary>
    public static LyricsEntity CreateDefault() =>
        new LyricsBuilder()
            .WithSongTitle(TestConstants.Content.Editorial.Lyrics.ValidSongTitle)
            .WithArtistName(TestConstants.Content.Editorial.Lyrics.ValidArtistName)
            .Build();
}
