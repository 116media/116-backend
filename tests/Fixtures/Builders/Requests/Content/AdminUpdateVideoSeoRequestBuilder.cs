using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo.V1;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateVideoSeoRequest"/> instances in tests
/// with valid default values that satisfy the update video SEO validator.
/// </summary>
public class AdminUpdateVideoSeoRequestBuilder
{
    private string? _metaTitle;
    private string? _metaDescription;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateVideoSeoRequestBuilder"/> class
    /// with valid values that satisfy the validator (meta title 10–70 chars, meta description 50–160 chars).
    /// </summary>
    public AdminUpdateVideoSeoRequestBuilder()
    {
        _metaTitle = "116 Le Focus Fally";
        _metaDescription =
            "Épisode complet de 116 Le Focus avec Fally Ipupa, une conversation exclusive sur sa carrière et ses projets.";
    }

    /// <summary>
    /// Sets the optional SEO meta title.
    /// </summary>
    /// <param name="metaTitle">The meta title, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoSeoRequestBuilder WithMetaTitle(string? metaTitle)
    {
        _metaTitle = metaTitle;
        return this;
    }

    /// <summary>
    /// Sets the optional SEO meta description.
    /// </summary>
    /// <param name="metaDescription">The meta description, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoSeoRequestBuilder WithMetaDescription(string? metaDescription)
    {
        _metaDescription = metaDescription;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateVideoSeoRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateVideoSeoRequest instance.</returns>
    public AdminUpdateVideoSeoRequest Build()
    {
        return new AdminUpdateVideoSeoRequest(MetaTitle: _metaTitle, MetaDescription: _metaDescription);
    }
}
