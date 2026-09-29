using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertAlbumStreamingLink.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpsertAlbumStreamingLinkRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminUpsertAlbumStreamingLinkRequestBuilder
{
    private string _url;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpsertAlbumStreamingLinkRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminUpsertAlbumStreamingLinkRequestBuilder()
    {
        _url = "https://open.spotify.com/album/first-curated";
    }

    /// <summary>
    /// Sets the url.
    /// </summary>
    /// <param name="url">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpsertAlbumStreamingLinkRequestBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpsertAlbumStreamingLinkRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpsertAlbumStreamingLinkRequest instance.</returns>
    public AdminUpsertAlbumStreamingLinkRequest Build()
    {
        return new AdminUpsertAlbumStreamingLinkRequest(Url: _url);
    }
}
