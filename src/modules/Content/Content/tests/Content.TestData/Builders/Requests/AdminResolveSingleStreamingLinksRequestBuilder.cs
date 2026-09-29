using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminResolveSingleStreamingLinksRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminResolveSingleStreamingLinksRequestBuilder
{
    private string _sourceUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminResolveSingleStreamingLinksRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminResolveSingleStreamingLinksRequestBuilder()
    {
        _sourceUrl = "https://open.spotify.com/track/xyz789";
    }

    /// <summary>
    /// Sets the source url.
    /// </summary>
    /// <param name="sourceUrl">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminResolveSingleStreamingLinksRequestBuilder WithSourceUrl(string sourceUrl)
    {
        _sourceUrl = sourceUrl;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminResolveSingleStreamingLinksRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminResolveSingleStreamingLinksRequest instance.</returns>
    public AdminResolveSingleStreamingLinksRequest Build()
    {
        return new AdminResolveSingleStreamingLinksRequest(SourceUrl: _sourceUrl);
    }
}
