using _116.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminSetArticleArtistsRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminSetArticleArtistsRequestBuilder
{
    private IReadOnlyList<Guid> _artistIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSetArticleArtistsRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminSetArticleArtistsRequestBuilder()
    {
        _artistIds = [];
    }

    /// <summary>
    /// Sets the artist ids.
    /// </summary>
    /// <param name="artistIds">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminSetArticleArtistsRequestBuilder WithArtistIds(IReadOnlyList<Guid> artistIds)
    {
        _artistIds = artistIds;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminSetArticleArtistsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminSetArticleArtistsRequest instance.</returns>
    public AdminSetArticleArtistsRequest Build()
    {
        return new AdminSetArticleArtistsRequest(ArtistIds: _artistIds);
    }
}
