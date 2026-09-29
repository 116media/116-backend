using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum.V1;
using _116.Content.Domain.Enums;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateAlbumRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminUpdateAlbumRequestBuilder
{
    private string _name;
    private short? _releaseYear;
    private string? _label;
    private EnumReleaseType _releaseType;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateAlbumRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminUpdateAlbumRequestBuilder()
    {
        _name = "Name";
        _releaseYear = null;
        _label = null;
        _releaseType = EnumReleaseType.Album;
    }

    /// <summary>
    /// Sets the name.
    /// </summary>
    /// <param name="name">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateAlbumRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the release year.
    /// </summary>
    /// <param name="releaseYear">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateAlbumRequestBuilder WithReleaseYear(short? releaseYear)
    {
        _releaseYear = releaseYear;
        return this;
    }

    /// <summary>
    /// Sets the label.
    /// </summary>
    /// <param name="label">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateAlbumRequestBuilder WithLabel(string? label)
    {
        _label = label;
        return this;
    }

    /// <summary>
    /// Sets the release type.
    /// </summary>
    /// <param name="releaseType">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateAlbumRequestBuilder WithReleaseType(EnumReleaseType releaseType)
    {
        _releaseType = releaseType;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateAlbumRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateAlbumRequest instance.</returns>
    public AdminUpdateAlbumRequest Build()
    {
        return new AdminUpdateAlbumRequest(
            Name: _name,
            ReleaseYear: _releaseYear,
            Label: _label,
            ReleaseType: _releaseType
        );
    }
}
