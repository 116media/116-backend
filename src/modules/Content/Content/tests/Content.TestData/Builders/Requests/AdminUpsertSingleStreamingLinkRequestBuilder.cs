using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpsertSingleStreamingLinkRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminUpsertSingleStreamingLinkRequestBuilder
{
    private string _url;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpsertSingleStreamingLinkRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminUpsertSingleStreamingLinkRequestBuilder()
    {
        _url = "https://open.spotify.com/track/first-curated";
    }

    /// <summary>
    /// Sets the url.
    /// </summary>
    /// <param name="url">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpsertSingleStreamingLinkRequestBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpsertSingleStreamingLinkRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpsertSingleStreamingLinkRequest instance.</returns>
    public AdminUpsertSingleStreamingLinkRequest Build()
    {
        return new AdminUpsertSingleStreamingLinkRequest(Url: _url);
    }
}
