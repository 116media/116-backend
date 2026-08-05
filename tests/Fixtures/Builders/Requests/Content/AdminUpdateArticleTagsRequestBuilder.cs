using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags.V1;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateArticleTagsRequest"/> instances in tests
/// with valid default values that satisfy the update article tags validator.
/// </summary>
public class AdminUpdateArticleTagsRequestBuilder
{
    private IReadOnlyList<string> _tagNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateArticleTagsRequestBuilder"/> class
    /// with valid default tag names that satisfy the validator.
    /// </summary>
    public AdminUpdateArticleTagsRequestBuilder()
    {
        _tagNames = new List<string> { TestConstants.Tag.ValidName, TestConstants.Tag.AnotherValidName };
    }

    /// <summary>
    /// Sets the complete set of tag display names to assign to the article.
    /// </summary>
    /// <param name="tagNames">The tag names.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleTagsRequestBuilder WithTagNames(IReadOnlyList<string> tagNames)
    {
        _tagNames = tagNames;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateArticleTagsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateArticleTagsRequest instance.</returns>
    public AdminUpdateArticleTagsRequest Build()
    {
        return new AdminUpdateArticleTagsRequest(TagNames: _tagNames);
    }
}
