using _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsArtist.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminLinkLyricsArtistRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminLinkLyricsArtistRequestBuilder
{
    private Guid? _artistId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminLinkLyricsArtistRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminLinkLyricsArtistRequestBuilder()
    {
        _artistId = null;
    }

    /// <summary>
    /// Sets the artist id.
    /// </summary>
    /// <param name="artistId">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminLinkLyricsArtistRequestBuilder WithArtistId(Guid? artistId)
    {
        _artistId = artistId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminLinkLyricsArtistRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminLinkLyricsArtistRequest instance.</returns>
    public AdminLinkLyricsArtistRequest Build()
    {
        return new AdminLinkLyricsArtistRequest(ArtistId: _artistId);
    }
}
