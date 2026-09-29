using _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkVideoArtist.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminLinkVideoArtistRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminLinkVideoArtistRequestBuilder
{
    private Guid? _artistId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminLinkVideoArtistRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminLinkVideoArtistRequestBuilder()
    {
        _artistId = null;
    }

    /// <summary>
    /// Sets the artist id.
    /// </summary>
    /// <param name="artistId">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminLinkVideoArtistRequestBuilder WithArtistId(Guid? artistId)
    {
        _artistId = artistId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminLinkVideoArtistRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminLinkVideoArtistRequest instance.</returns>
    public AdminLinkVideoArtistRequest Build()
    {
        return new AdminLinkVideoArtistRequest(ArtistId: _artistId);
    }
}
