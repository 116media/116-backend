using _116.Content.Application.Editorial.UseCases.Admin.Commands.SetLyricsTags.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminSetLyricsTagsRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminSetLyricsTagsRequestBuilder
{
    private IReadOnlyCollection<Guid> _tagIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSetLyricsTagsRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminSetLyricsTagsRequestBuilder()
    {
        _tagIds = [];
    }

    /// <summary>
    /// Sets the tag ids.
    /// </summary>
    /// <param name="tagIds">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminSetLyricsTagsRequestBuilder WithTagIds(IReadOnlyCollection<Guid> tagIds)
    {
        _tagIds = tagIds;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminSetLyricsTagsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminSetLyricsTagsRequest instance.</returns>
    public AdminSetLyricsTagsRequest Build()
    {
        return new AdminSetLyricsTagsRequest(TagIds: _tagIds);
    }
}
