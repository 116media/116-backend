using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.V1;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateLyricsRequest"/> instances in tests
/// with valid default values that satisfy the update lyrics validator.
/// </summary>
public class AdminUpdateLyricsRequestBuilder
{
    private readonly Faker _faker = new();

    private Guid _categoryId;
    private string _songTitle;
    private string _artistName;
    private string _slug;
    private string _lyricsText;
    private string _language;
    private Guid? _videoId;
    private Guid? _customerId;
    private Guid? _orderItemId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateLyricsRequestBuilder"/> class
    /// with valid default values that satisfy the validator.
    /// </summary>
    public AdminUpdateLyricsRequestBuilder()
    {
        _categoryId = _faker.Random.Guid();
        _songTitle = TestConstants.Content.Editorial.Lyrics.ValidSongTitle;
        _artistName = TestConstants.Content.Editorial.Lyrics.ValidArtistName;
        _slug = TestConstants.Content.Editorial.Lyrics.ValidSlug;
        _lyricsText = TestConstants.Content.Editorial.Lyrics.ValidLyricsText;
        _language = TestConstants.Content.Editorial.Lyrics.ValidLanguage;
        _videoId = null;
        _customerId = null;
        _orderItemId = null;
    }

    /// <summary>
    /// Sets the category the lyrics page belongs to.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsRequestBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
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
    /// Sets the URL-safe slug for the lyrics page.
    /// </summary>
    /// <param name="slug">The lyrics page slug.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsRequestBuilder WithSlug(string slug)
    {
        _slug = slug;
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
    /// Sets the optional B2B customer who commissioned the lyrics page.
    /// </summary>
    /// <param name="customerId">The customer identifier, or null for free content.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsRequestBuilder WithCustomerId(Guid? customerId)
    {
        _customerId = customerId;
        return this;
    }

    /// <summary>
    /// Sets the optional order item the lyrics page fulfils.
    /// </summary>
    /// <param name="orderItemId">The order item identifier, or null for free content.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsRequestBuilder WithOrderItemId(Guid? orderItemId)
    {
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateLyricsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateLyricsRequest instance.</returns>
    public AdminUpdateLyricsRequest Build()
    {
        return new AdminUpdateLyricsRequest(
            CategoryId: _categoryId,
            SongTitle: _songTitle,
            ArtistName: _artistName,
            Slug: _slug,
            LyricsText: _lyricsText,
            Language: _language,
            VideoId: _videoId,
            CustomerId: _customerId,
            OrderItemId: _orderItemId
        );
    }
}
