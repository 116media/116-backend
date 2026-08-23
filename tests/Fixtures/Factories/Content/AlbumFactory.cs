using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="AlbumBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class AlbumFactory
{
    /// <summary>
    /// Creates a standalone album (no linked artist) with default valid values.
    /// </summary>
    public static AlbumEntity Create() => new AlbumBuilder().Build();

    /// <summary>
    /// Creates an album linked to the given artist profile.
    /// </summary>
    public static AlbumEntity CreateForArtist(Guid artistId) => new AlbumBuilder().WithArtistId(artistId).Build();

    /// <summary>
    /// Creates an album of a specific release type linked to an artist.
    /// </summary>
    public static AlbumEntity CreateForArtist(Guid artistId, EnumReleaseType releaseType) =>
        new AlbumBuilder().WithArtistId(artistId).WithReleaseType(releaseType).Build();

    /// <summary>
    /// Creates an album of a specific release type, name and year linked to an artist.
    /// </summary>
    public static AlbumEntity CreateForArtist(Guid artistId, EnumReleaseType releaseType, string name, short? year) =>
        new AlbumBuilder()
            .WithArtistId(artistId)
            .WithReleaseType(releaseType)
            .WithName(name)
            .WithReleaseYearOrNull(year)
            .Build();

    /// <summary>
    /// Creates an album with a specific name.
    /// </summary>
    public static AlbumEntity CreateWithName(string name) => new AlbumBuilder().WithName(name).Build();

    /// <summary>
    /// Creates an album with a cover image file id set.
    /// </summary>
    public static AlbumEntity CreateWithCoverImageFileId(Guid coverImageFileId) =>
        new AlbumBuilder().WithCoverImageFileId(coverImageFileId).Build();

    /// <summary>
    /// Creates a list of standalone albums.
    /// </summary>
    public static List<AlbumEntity> CreateMany(int count) => Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
