using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpsertArtistSocialLinkRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminUpsertArtistSocialLinkRequestBuilder
{
    private string _url;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpsertArtistSocialLinkRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminUpsertArtistSocialLinkRequestBuilder()
    {
        _url = "https://instagram.com/fallyipupa01";
    }

    /// <summary>
    /// Sets the url.
    /// </summary>
    /// <param name="url">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpsertArtistSocialLinkRequestBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpsertArtistSocialLinkRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpsertArtistSocialLinkRequest instance.</returns>
    public AdminUpsertArtistSocialLinkRequest Build()
    {
        return new AdminUpsertArtistSocialLinkRequest(Url: _url);
    }
}
