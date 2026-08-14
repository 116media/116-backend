using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="AlbumEntity"/> instances in tests.
/// For test code, prefer using AlbumFactory instead of direct Builder usage.
/// </summary>
internal class AlbumBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = TestConstants.Content.Editorial.Album.ValidName;
    private Guid? _artistId;
    private Guid? _coverImageFileId;
    private short? _releaseYear;
    private string? _label;
    private EnumReleaseType _releaseType = EnumReleaseType.Album;

    /// <summary>
    /// Sets the album ID.
    /// </summary>
    public AlbumBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the album's display name.
    /// </summary>
    public AlbumBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Links the album to an artist profile.
    /// </summary>
    public AlbumBuilder WithArtistId(Guid artistId)
    {
        _artistId = artistId;
        return this;
    }

    /// <summary>
    /// Sets the album's cover image file ID.
    /// </summary>
    public AlbumBuilder WithCoverImageFileId(Guid coverImageFileId)
    {
        _coverImageFileId = coverImageFileId;
        return this;
    }

    /// <summary>
    /// Sets the album's release year.
    /// </summary>
    public AlbumBuilder WithReleaseYear(short releaseYear)
    {
        _releaseYear = releaseYear;
        return this;
    }

    /// <summary>
    /// Sets the album's record label.
    /// </summary>
    public AlbumBuilder WithLabel(string label)
    {
        _label = label;
        return this;
    }

    /// <summary>
    /// Sets the album's release year, allowing null for an undated release.
    /// </summary>
    public AlbumBuilder WithReleaseYearOrNull(short? releaseYear)
    {
        _releaseYear = releaseYear;
        return this;
    }

    /// <summary>
    /// Sets the album's release type.
    /// </summary>
    public AlbumBuilder WithReleaseType(EnumReleaseType releaseType)
    {
        _releaseType = releaseType;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AlbumEntity"/> instance.
    /// </summary>
    public AlbumEntity Build()
    {
        AlbumEntity entity = AlbumEntity.Create(
            id: _id,
            name: _name,
            artistId: _artistId,
            coverImageFileId: _coverImageFileId,
            releaseYear: _releaseYear,
            label: _label,
            releaseType: _releaseType,
            errors: TestErrorsFactory.CreateAlbumErrors()
        );

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }
}
