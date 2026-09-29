using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminResolveAlbumStreamingLinksRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminResolveAlbumStreamingLinksRequestBuilder
{
    private string _sourceUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminResolveAlbumStreamingLinksRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminResolveAlbumStreamingLinksRequestBuilder()
    {
        _sourceUrl = "https://open.spotify.com/album/abc123";
    }

    /// <summary>
    /// Sets the source url.
    /// </summary>
    /// <param name="sourceUrl">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminResolveAlbumStreamingLinksRequestBuilder WithSourceUrl(string sourceUrl)
    {
        _sourceUrl = sourceUrl;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminResolveAlbumStreamingLinksRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminResolveAlbumStreamingLinksRequest instance.</returns>
    public AdminResolveAlbumStreamingLinksRequest Build()
    {
        return new AdminResolveAlbumStreamingLinksRequest(SourceUrl: _sourceUrl);
    }
}
