using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a song lyrics page on the platform. Lyrics pages are SEO-optimised standalone
/// pages that target keyword searches (e.g., "Fally Ipupa — Eloko Oyo lyrics").
/// <para>
/// A lyrics entry can be:
/// <list type="bullet">
///   <item>Linked to a <see cref="VideoEntity" /> (e.g., a lyric video or "Behind the Lyrics" episode)</item>
///   <item>Linked to an <see cref="ArticleEntity" /> (e.g., a dedicated Lyrics Page article)</item>
///   <item>Standalone with no parent content</item>
/// </list>
/// </para>
/// </summary>
public class LyricsEntity : Aggregate<Guid>
{
    /// <summary>
    /// Optional link to a parent video. <c>null</c> unless this lyrics page is
    /// associated with a lyric video or a "Behind the Lyrics" episode.
    /// </summary>
    public Guid? VideoId { get; private set; }

    /// <summary>
    /// Optional link to a parent article. <c>null</c> unless this lyrics page
    /// is the content body of a "Lyrics Page" article type.
    /// </summary>
    public Guid? ArticleId { get; private set; }

    /// <summary>
    /// The title of the song.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxSongTitleLength)]
    public string SongTitle { get; private set; } = null!;

    /// <summary>
    /// The name of the performing artist.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxArtistNameLength)]
    public string ArtistName { get; private set; } = null!;

    /// <summary>
    /// The full lyrics text of the song.
    /// </summary>
    public string LyricsText { get; private set; } = null!;

    /// <summary>
    /// ISO 639-1 language code (e.g., "fr", "ln", "en"). BCP-47 subtags supported (e.g., "fr-CD").
    /// Defaults to "fr" for Lingala/French content.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxLyricsLanguageLength)]
    public string Language { get; private set; } = ContentConstants.DefaultLyricsLanguage;

    /// <summary>
    /// Custom SEO meta title (max 70 chars).
    /// </summary>
    [MaxLength(length: ContentConstants.MaxMetaTitleLength)]
    public string? MetaTitle { get; private set; }

    /// <summary>
    /// Custom SEO meta description (max 160 chars).
    /// </summary>
    [MaxLength(length: ContentConstants.MaxMetaDescriptionLength)]
    public string? MetaDescription { get; private set; }

    /// <summary>
    /// SEO meta keywords (comma-separated, max 300 chars).
    /// </summary>
    [MaxLength(length: ContentConstants.MaxMetaKeywordsLength)]
    public string? MetaKeywords { get; private set; }

    /// <summary>
    /// Schema.org JSON-LD structured data for enhanced Google search results.
    /// Stored as JSONB in PostgreSQL. Generated automatically or provided manually.
    /// </summary>
    public string? StructuredData { get; private set; }

    /// <summary>
    /// The parent video this lyrics page is linked to. <c>null</c> if standalone or article-linked.
    /// </summary>
    public VideoEntity? Video { get; private set; }

    /// <summary>
    /// The parent article this lyrics page is linked to. <c>null</c> if standalone or video-linked.
    /// </summary>
    public ArticleEntity? Article { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private LyricsEntity() { }

    /// <summary>
    /// Creates a lyrics page linked to a video (e.g., lyric video, "Behind the Lyrics" episode).
    /// </summary>
    public static LyricsEntity CreateForVideo(
        Guid id,
        Guid videoId,
        string songTitle,
        string artistName,
        string lyricsText,
        string language
    )
    {
        ValidateRequiredFields(songTitle: songTitle, artistName: artistName, lyricsText: lyricsText);

        return new LyricsEntity
        {
            Id = id,
            VideoId = videoId,
            SongTitle = songTitle,
            ArtistName = artistName,
            LyricsText = lyricsText,
            Language = language,
        };
    }

    /// <summary>
    /// Creates a lyrics page linked to an article (e.g., a dedicated "Lyrics Page" article type).
    /// </summary>
    public static LyricsEntity CreateForArticle(
        Guid id,
        Guid articleId,
        string songTitle,
        string artistName,
        string lyricsText,
        string language
    )
    {
        ValidateRequiredFields(songTitle: songTitle, artistName: artistName, lyricsText: lyricsText);

        return new LyricsEntity
        {
            Id = id,
            ArticleId = articleId,
            SongTitle = songTitle,
            ArtistName = artistName,
            LyricsText = lyricsText,
            Language = language,
        };
    }

    /// <summary>
    /// Creates a standalone lyrics page not linked to any parent content.
    /// </summary>
    public static LyricsEntity CreateStandalone(
        Guid id,
        string songTitle,
        string artistName,
        string lyricsText,
        string language
    )
    {
        ValidateRequiredFields(songTitle: songTitle, artistName: artistName, lyricsText: lyricsText);

        return new LyricsEntity
        {
            Id = id,
            SongTitle = songTitle,
            ArtistName = artistName,
            LyricsText = lyricsText,
            Language = language,
        };
    }

    /// <summary>
    /// Replaces the lyrics text.
    /// </summary>
    public void UpdateLyrics(string lyricsText)
    {
        if (string.IsNullOrWhiteSpace(value: lyricsText))
        {
            throw LyricsErrors.LyricsTextRequired();
        }

        LyricsText = lyricsText;
    }

    /// <summary>
    /// Updates the SEO metadata for this lyrics page.
    /// </summary>
    public void UpdateSeo(string? metaTitle, string? metaDescription, string? metaKeywords, string? structuredData)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        StructuredData = structuredData;
    }

    private static void ValidateRequiredFields(string songTitle, string artistName, string lyricsText)
    {
        if (string.IsNullOrWhiteSpace(value: songTitle))
        {
            throw LyricsErrors.SongTitleRequired();
        }

        if (string.IsNullOrWhiteSpace(value: artistName))
        {
            throw LyricsErrors.ArtistNameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: lyricsText))
        {
            throw LyricsErrors.LyricsTextRequired();
        }
    }
}
