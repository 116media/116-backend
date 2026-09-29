using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateLyricsMetadataRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminUpdateLyricsMetadataRequestBuilder
{
    private string? _album;
    private short? _releaseYear;
    private string? _label;
    private string? _songwriter;
    private string? _producer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateLyricsMetadataRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminUpdateLyricsMetadataRequestBuilder()
    {
        _album = null;
        _releaseYear = null;
        _label = null;
        _songwriter = null;
        _producer = null;
    }

    /// <summary>
    /// Sets the album.
    /// </summary>
    /// <param name="album">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsMetadataRequestBuilder WithAlbum(string? album)
    {
        _album = album;
        return this;
    }

    /// <summary>
    /// Sets the release year.
    /// </summary>
    /// <param name="releaseYear">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsMetadataRequestBuilder WithReleaseYear(short? releaseYear)
    {
        _releaseYear = releaseYear;
        return this;
    }

    /// <summary>
    /// Sets the label.
    /// </summary>
    /// <param name="label">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsMetadataRequestBuilder WithLabel(string? label)
    {
        _label = label;
        return this;
    }

    /// <summary>
    /// Sets the songwriter.
    /// </summary>
    /// <param name="songwriter">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsMetadataRequestBuilder WithSongwriter(string? songwriter)
    {
        _songwriter = songwriter;
        return this;
    }

    /// <summary>
    /// Sets the producer.
    /// </summary>
    /// <param name="producer">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsMetadataRequestBuilder WithProducer(string? producer)
    {
        _producer = producer;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateLyricsMetadataRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateLyricsMetadataRequest instance.</returns>
    public AdminUpdateLyricsMetadataRequest Build()
    {
        return new AdminUpdateLyricsMetadataRequest(
            Album: _album,
            ReleaseYear: _releaseYear,
            Label: _label,
            Songwriter: _songwriter,
            Producer: _producer
        );
    }
}
