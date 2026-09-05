using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a real, addressable album — distinct from the plain-text <c>Album</c> field
/// on <see cref="LyricsEntity" />. Groups songs for "more from this album" browsing and
/// carries per-album cover art and release metadata. Unlike <see cref="ArtistEntity" />,
/// albums have no ownership/claiming concept — an album is either staff-curated or it isn't.
/// </summary>
public class AlbumEntity : Aggregate<Guid>
{
    /// <summary>
    /// Display name of the album.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxAlbumNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional link to the claimed artist profile this album belongs to. Null if the
    /// album isn't yet associated with a real <see cref="ArtistEntity" /> profile.
    /// </summary>
    public Guid? ArtistId { get; private set; }

    /// <summary>
    /// ID of the uploaded cover art file tracked in the Core module. Null until uploaded.
    /// </summary>
    public Guid? CoverImageFileId { get; private set; }

    /// <summary>
    /// The year the album was released, if known.
    /// </summary>
    public short? ReleaseYear { get; private set; }

    /// <summary>
    /// The record label that released the album, if known.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxLabelNameLength)]
    public string? Label { get; private set; }

    /// <summary>
    /// What kind of release this is. Non-nullable with a default of
    /// <see cref="EnumReleaseType.Album" /> — an untyped release would fall out of every
    /// profile section and silently disappear, so every row is always in exactly one bucket.
    /// </summary>
    public EnumReleaseType ReleaseType { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private AlbumEntity() { }

    /// <summary>
    /// Creates a new album.
    /// </summary>
    /// <param name="id">The unique identifier for the album.</param>
    /// <param name="name">The album's display name.</param>
    /// <param name="artistId">Optional link to the claimed artist profile.</param>
    /// <param name="coverImageFileId">Optional cover art file reference.</param>
    /// <param name="releaseYear">The release year, if known.</param>
    /// <param name="label">The record label, if known.</param>
    /// <param name="releaseType">What kind of release this is.</param>
    /// <returns>A new <see cref="AlbumEntity" />.</returns>
    public static AlbumEntity Create(
        Guid id,
        string name,
        Guid? artistId,
        Guid? coverImageFileId,
        short? releaseYear,
        string? label,
        EnumReleaseType releaseType
    )
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.AlbumNameRequired);
        }

        return new AlbumEntity
        {
            Id = id,
            Name = name,
            ArtistId = artistId,
            CoverImageFileId = coverImageFileId,
            ReleaseYear = releaseYear,
            Label = label,
            ReleaseType = releaseType,
        };
    }

    /// <summary>
    /// Updates the album's editable fields in a single call.
    /// </summary>
    /// <param name="name">The album's display name.</param>
    /// <param name="coverImageFileId">Optional cover art file reference, or null to clear it.</param>
    /// <param name="releaseYear">The release year, or null to clear it.</param>
    /// <param name="label">The record label, or null to clear it.</param>
    /// <param name="releaseType">What kind of release this is.</param>
    public void Update(
        string name,
        Guid? coverImageFileId,
        short? releaseYear,
        string? label,
        EnumReleaseType releaseType
    )
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.AlbumNameRequired);
        }

        Name = name;
        CoverImageFileId = coverImageFileId;
        ReleaseYear = releaseYear;
        Label = label;
        ReleaseType = releaseType;
    }
}
