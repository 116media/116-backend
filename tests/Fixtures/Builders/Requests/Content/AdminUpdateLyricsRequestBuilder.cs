using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.V1;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateLyricsRequest"/> instances in tests
/// with valid default values that satisfy the update lyrics validator.
/// </summary>
public class AdminUpdateLyricsRequestBuilder
{
    private string _songTitle;
    private string _artistName;
    private string _lyricsText;
    private string _language;
    private Guid? _videoId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateLyricsRequestBuilder"/> class
    /// with valid default values that satisfy the validator.
    /// </summary>
    public AdminUpdateLyricsRequestBuilder()
    {
        _songTitle = TestConstants.Content.Editorial.Lyrics.ValidSongTitle;
        _artistName = TestConstants.Content.Editorial.Lyrics.ValidArtistName;
        _lyricsText = TestConstants.Content.Editorial.Lyrics.ValidLyricsText;
        _language = TestConstants.Content.Editorial.Lyrics.ValidLanguage;
        _videoId = null;
    }

    /// <summary>
    /// Sets the song title.
    /// </summary>
    /// <param name="songTitle">The song title.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsRequestBuilder WithSongTitle(string songTitle)
    {
        _songTitle = songTitle;
        return this;
    }

    /// <summary>
    /// Sets the artist name.
    /// </summary>
    /// <param name="artistName">The artist name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsRequestBuilder WithArtistName(string artistName)
    {
        _artistName = artistName;
        return this;
    }

    /// <summary>
    /// Sets the lyrics text.
    /// </summary>
    /// <param name="lyricsText">The lyrics text.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsRequestBuilder WithLyricsText(string lyricsText)
    {
        _lyricsText = lyricsText;
        return this;
    }

    /// <summary>
    /// Sets the lyrics language code.
    /// </summary>
    /// <param name="language">The language code.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsRequestBuilder WithLanguage(string language)
    {
        _language = language;
        return this;
    }

    /// <summary>
    /// Sets the optional video the lyrics are associated with.
    /// </summary>
    /// <param name="videoId">The video identifier, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsRequestBuilder WithVideoId(Guid? videoId)
    {
        _videoId = videoId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateLyricsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateLyricsRequest instance.</returns>
    public AdminUpdateLyricsRequest Build()
    {
        return new AdminUpdateLyricsRequest(
            SongTitle: _songTitle,
            ArtistName: _artistName,
            LyricsText: _lyricsText,
            Language: _language,
            VideoId: _videoId
        );
    }
}
