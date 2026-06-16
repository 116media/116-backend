using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo.V1;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateLyricsSeoRequest"/> instances in tests
/// with valid default values that satisfy the update lyrics SEO validator.
/// </summary>
public class AdminUpdateLyricsSeoRequestBuilder
{
    private string? _metaTitle;
    private string? _metaDescription;
    private string? _structuredData;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateLyricsSeoRequestBuilder"/> class
    /// with valid default SEO metadata values.
    /// </summary>
    public AdminUpdateLyricsSeoRequestBuilder()
    {
        _metaTitle = "Eloko Oyo — Paroles";
        _metaDescription = "Découvrez les paroles complètes de la chanson Eloko Oyo de Fally Ipupa.";
        _structuredData = "{\"@context\":\"https://schema.org\",\"@type\":\"MusicComposition\"}";
    }

    /// <summary>
    /// Sets the optional SEO meta title.
    /// </summary>
    /// <param name="metaTitle">The meta title, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsSeoRequestBuilder WithMetaTitle(string? metaTitle)
    {
        _metaTitle = metaTitle;
        return this;
    }

    /// <summary>
    /// Sets the optional SEO meta description.
    /// </summary>
    /// <param name="metaDescription">The meta description, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsSeoRequestBuilder WithMetaDescription(string? metaDescription)
    {
        _metaDescription = metaDescription;
        return this;
    }

    /// <summary>
    /// Sets the optional schema.org JSON-LD structured data.
    /// </summary>
    /// <param name="structuredData">The structured data, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateLyricsSeoRequestBuilder WithStructuredData(string? structuredData)
    {
        _structuredData = structuredData;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateLyricsSeoRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateLyricsSeoRequest instance.</returns>
    public AdminUpdateLyricsSeoRequest Build()
    {
        return new AdminUpdateLyricsSeoRequest(
            MetaTitle: _metaTitle,
            MetaDescription: _metaDescription,
            StructuredData: _structuredData
        );
    }
}
