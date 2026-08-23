using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags.V1;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateVideoTagsRequest"/> instances in tests
/// with valid default values that satisfy the update video tags validator.
/// </summary>
public class AdminUpdateVideoTagsRequestBuilder
{
    private IReadOnlyList<string> _tagNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateVideoTagsRequestBuilder"/> class
    /// with valid default tag names that satisfy the validator.
    /// </summary>
    public AdminUpdateVideoTagsRequestBuilder()
    {
        _tagNames = new List<string> { TestConstants.Tag.ValidName, TestConstants.Tag.AnotherValidName };
    }

    /// <summary>
    /// Sets the complete set of tag display names to assign to the video.
    /// </summary>
    /// <param name="tagNames">The tag names.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoTagsRequestBuilder WithTagNames(IReadOnlyList<string> tagNames)
    {
        _tagNames = tagNames;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateVideoTagsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateVideoTagsRequest instance.</returns>
    public AdminUpdateVideoTagsRequest Build()
    {
        return new AdminUpdateVideoTagsRequest(TagNames: _tagNames);
    }
}
