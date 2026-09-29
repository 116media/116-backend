using _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsAlbum.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminLinkLyricsAlbumRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminLinkLyricsAlbumRequestBuilder
{
    private Guid? _albumId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminLinkLyricsAlbumRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminLinkLyricsAlbumRequestBuilder()
    {
        _albumId = null;
    }

    /// <summary>
    /// Sets the album id.
    /// </summary>
    /// <param name="albumId">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminLinkLyricsAlbumRequestBuilder WithAlbumId(Guid? albumId)
    {
        _albumId = albumId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminLinkLyricsAlbumRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminLinkLyricsAlbumRequest instance.</returns>
    public AdminLinkLyricsAlbumRequest Build()
    {
        return new AdminLinkLyricsAlbumRequest(AlbumId: _albumId);
    }
}
