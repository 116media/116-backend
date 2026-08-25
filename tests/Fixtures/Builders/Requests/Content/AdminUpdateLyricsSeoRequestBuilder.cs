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
