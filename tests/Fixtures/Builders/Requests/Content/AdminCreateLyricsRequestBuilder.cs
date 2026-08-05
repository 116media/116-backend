using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreateLyricsRequest"/> instances in tests
/// with valid default values that satisfy the create lyrics validator.
/// </summary>
public class AdminCreateLyricsRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

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
    /// Initializes a new instance of the <see cref="AdminCreateLyricsRequestBuilder"/> class
    /// with valid default values that satisfy the validator.
    /// </summary>
    public AdminCreateLyricsRequestBuilder()
    {
        _categoryId = _faker.Random.Guid();
        _songTitle = TestConstants.Lyrics.ValidSongTitle;
        _artistName = TestConstants.Lyrics.ValidArtistName;
        _slug = TestConstants.Lyrics.ValidSlug;
        _lyricsText = TestConstants.Lyrics.ValidLyricsText;
        _language = TestConstants.Lyrics.ValidLanguage;
        _videoId = null;
        _customerId = null;
        _orderItemId = null;
    }

    /// <summary>
    /// Sets the category the lyrics page belongs to.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateLyricsRequestBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
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
    /// Sets the URL-safe slug for the lyrics page.
    /// </summary>
    /// <param name="slug">The lyrics page slug.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateLyricsRequestBuilder WithSlug(string slug)
    {
        _slug = slug;
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
    /// Sets the optional B2B customer who commissioned the lyrics page.
    /// </summary>
    /// <param name="customerId">The customer identifier, or null for free content.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateLyricsRequestBuilder WithCustomerId(Guid? customerId)
    {
        _customerId = customerId;
        return this;
    }

    /// <summary>
    /// Sets the optional order item the lyrics page fulfils.
    /// </summary>
    /// <param name="orderItemId">The order item identifier, or null for free content.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateLyricsRequestBuilder WithOrderItemId(Guid? orderItemId)
    {
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateLyricsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateLyricsRequest instance.</returns>
    public AdminCreateLyricsRequest Build()
    {
        return new AdminCreateLyricsRequest(
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
