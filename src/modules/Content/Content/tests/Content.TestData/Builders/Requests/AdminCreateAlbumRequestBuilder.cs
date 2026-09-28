using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum.V1;
using _116.Content.Domain.Enums;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreateAlbumRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminCreateAlbumRequestBuilder
{
    private string _name;
    private Guid? _artistId;
    private short? _releaseYear;
    private string? _label;
    private EnumReleaseType _releaseType;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreateAlbumRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminCreateAlbumRequestBuilder()
    {
        _name = "Album Name";
        _artistId = null;
        _releaseYear = null;
        _label = null;
        _releaseType = EnumReleaseType.Album;
    }

    /// <summary>
    /// Sets the name.
    /// </summary>
    /// <param name="name">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateAlbumRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the artist id.
    /// </summary>
    /// <param name="artistId">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateAlbumRequestBuilder WithArtistId(Guid? artistId)
    {
        _artistId = artistId;
        return this;
    }

    /// <summary>
    /// Sets the release year.
    /// </summary>
    /// <param name="releaseYear">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateAlbumRequestBuilder WithReleaseYear(short? releaseYear)
    {
        _releaseYear = releaseYear;
        return this;
    }

    /// <summary>
    /// Sets the label.
    /// </summary>
    /// <param name="label">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateAlbumRequestBuilder WithLabel(string? label)
    {
        _label = label;
        return this;
    }

    /// <summary>
    /// Sets the release type.
    /// </summary>
    /// <param name="releaseType">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateAlbumRequestBuilder WithReleaseType(EnumReleaseType releaseType)
    {
        _releaseType = releaseType;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateAlbumRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateAlbumRequest instance.</returns>
    public AdminCreateAlbumRequest Build()
    {
        return new AdminCreateAlbumRequest(
            Name: _name,
            ArtistId: _artistId,
            ReleaseYear: _releaseYear,
            Label: _label,
            ReleaseType: _releaseType
        );
    }
}
