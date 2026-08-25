using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo.V1;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateArticleSeoRequest"/> instances in tests
/// with valid default values that satisfy the update article SEO validator.
/// </summary>
public class AdminUpdateArticleSeoRequestBuilder
{
    private string? _metaTitle;
    private string? _metaDescription;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateArticleSeoRequestBuilder"/> class
    /// with valid values that satisfy the validator (meta title 10–70 chars, meta description 50–160 chars).
    /// </summary>
    public AdminUpdateArticleSeoRequestBuilder()
    {
        _metaTitle = "Fally Ipupa Portrait";
        _metaDescription =
            "Un portrait détaillé de Fally Ipupa, sa carrière musicale et son influence sur la rumba congolaise moderne.";
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateArticleSeoRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateArticleSeoRequest instance.</returns>
    public AdminUpdateArticleSeoRequest Build()
    {
        return new AdminUpdateArticleSeoRequest(MetaTitle: _metaTitle, MetaDescription: _metaDescription);
    }
}
