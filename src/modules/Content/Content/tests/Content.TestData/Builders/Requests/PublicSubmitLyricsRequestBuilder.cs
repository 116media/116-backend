using _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="PublicSubmitLyricsRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class PublicSubmitLyricsRequestBuilder
{
    private string _songTitle;
    private string? _artistName;
    private string _lyricsText;
    private string _language;
    private string? _slug;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicSubmitLyricsRequestBuilder"/> class with valid default values.
    /// </summary>
    public PublicSubmitLyricsRequestBuilder()
    {
        _songTitle = "Eloko Oyo";
        _artistName = "Fally Ipupa";
        _lyricsText = "Some submitted lyrics text.";
        _language = "fr";
        _slug = null;
    }

    /// <summary>
    /// Sets the song title.
    /// </summary>
    /// <param name="songTitle">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSubmitLyricsRequestBuilder WithSongTitle(string songTitle)
    {
        _songTitle = songTitle;
        return this;
    }

    /// <summary>
    /// Sets the artist name.
    /// </summary>
    /// <param name="artistName">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSubmitLyricsRequestBuilder WithArtistName(string? artistName)
    {
        _artistName = artistName;
        return this;
    }

    /// <summary>
    /// Sets the lyrics text.
    /// </summary>
    /// <param name="lyricsText">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSubmitLyricsRequestBuilder WithLyricsText(string lyricsText)
    {
        _lyricsText = lyricsText;
        return this;
    }

    /// <summary>
    /// Sets the language.
    /// </summary>
    /// <param name="language">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSubmitLyricsRequestBuilder WithLanguage(string language)
    {
        _language = language;
        return this;
    }

    /// <summary>
    /// Sets the slug.
    /// </summary>
    /// <param name="slug">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSubmitLyricsRequestBuilder WithSlug(string? slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicSubmitLyricsRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicSubmitLyricsRequest instance.</returns>
    public PublicSubmitLyricsRequest Build()
    {
        return new PublicSubmitLyricsRequest(
            SongTitle: _songTitle,
            ArtistName: _artistName,
            LyricsText: _lyricsText,
            Language: _language,
            Slug: _slug
        );
    }
}
