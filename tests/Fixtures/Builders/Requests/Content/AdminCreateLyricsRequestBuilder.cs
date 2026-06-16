using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.V1;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreateLyricsRequest"/> instances in tests
/// with valid default values that satisfy the create lyrics validator.
/// </summary>
public class AdminCreateLyricsRequestBuilder
{
    private string _songTitle;
    private string _artistName;
    private string _lyricsText;
    private string _language;
    private Guid? _videoId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreateLyricsRequestBuilder"/> class
    /// with valid default values that satisfy the validator.
    /// </summary>
    public AdminCreateLyricsRequestBuilder()
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
    public AdminCreateLyricsRequestBuilder WithSongTitle(string songTitle)
    {
        _songTitle = songTitle;
        return this;
    }

    /// <summary>
    /// Sets the artist name.
    /// </summary>
    /// <param name="artistName">The artist name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateLyricsRequestBuilder WithArtistName(string artistName)
    {
        _artistName = artistName;
        return this;
    }

    /// <summary>
    /// Sets the lyrics text.
    /// </summary>
    /// <param name="lyricsText">The lyrics text.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateLyricsRequestBuilder WithLyricsText(string lyricsText)
    {
        _lyricsText = lyricsText;
        return this;
    }

    /// <summary>
    /// Sets the lyrics language code.
    /// </summary>
    /// <param name="language">The language code.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateLyricsRequestBuilder WithLanguage(string language)
    {
        _language = language;
        return this;
    }

    /// <summary>
    /// Sets the optional video the lyrics are associated with.
    /// </summary>
    /// <param name="videoId">The video identifier, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateLyricsRequestBuilder WithVideoId(Guid? videoId)
    {
        _videoId = videoId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateLyricsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateLyricsRequest instance.</returns>
    public AdminCreateLyricsRequest Build()
    {
        return new AdminCreateLyricsRequest(
            SongTitle: _songTitle,
            ArtistName: _artistName,
            LyricsText: _lyricsText,
            Language: _language,
            VideoId: _videoId
        );
    }
}
