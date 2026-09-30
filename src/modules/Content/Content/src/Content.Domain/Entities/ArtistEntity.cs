using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a real, addressable artist profile — distinct from the plain-text
/// <c>ArtistName</c> field on <see cref="LyricsEntity" /> and <see cref="VideoEntity" />.
/// A profile can exist unclaimed (staff-curated, no linked account) or claimed by a verified
/// artist account via <see cref="UserId" />.
/// </summary>
public partial class ArtistEntity : Aggregate<Guid>
{
    /// <summary>
    /// Display name of the artist (e.g., "Fally Ipupa").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxArtistNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// URL-safe slug for the artist's public page (e.g., "fally-ipupa"). Unique across all
    /// artists. Immutable after creation — <see cref="Update" /> never touches it — so
    /// public URLs never break once shared.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxSlugLength)]
    public Slug Slug { get; private set; } = null!;

    /// <summary>
    /// Free-text biography shown on the artist's public page. Null until curated.
    /// </summary>
    public string? Bio { get; private set; }

    /// <summary>
    /// <see cref="Name" /> with accents stripped, whitespace collapsed and cased up. Derived
    /// on create and on rename, never set directly. Sorting, bucketing and public search all
    /// read this one stored value so they cannot disagree about where a name belongs.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxArtistNameLength)]
    public string NameFolded { get; private set; } = string.Empty;

    /// <summary>
    /// First character of <see cref="NameFolded" /> when it is A-Z, otherwise
    /// <see cref="ContentConstants.NonAlphabeticLetterBucket" />. Drives the directory's
    /// letter filter and its available-letters rail.
    /// </summary>
    [MaxLength(length: 1)]
    public string InitialLetter { get; private set; } = ContentConstants.NonAlphabeticLetterBucket;

    /// <summary>
    /// The artist's legal or birth name, shown in the profile's identity block. Null when
    /// unknown, and suppressed by the client when it duplicates the stage name.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxArtistRealNameLength)]
    public string? RealName { get; private set; }

    /// <summary>
    /// Alternate names the artist is known by. Display-only and never queried, which is why
    /// this is a text array rather than a join table. Empty rather than null when there are
    /// none.
    /// </summary>
    public IReadOnlyList<string> Aliases => _aliases;

    /// <summary>
    /// Backing store for <see cref="Aliases" />, mapped to a Postgres text[] column.
    /// </summary>
    private List<string> _aliases = [];

    /// <summary>
    /// The artist's date of birth as a civil date. Deliberately not a timestamp: a
    /// <c>DateTimeOffset</c> converted across timezones moves a birthday by a day.
    /// The displayed age is derived at render time and never stored.
    /// </summary>
    public DateOnly? Birthdate { get; private set; }

    /// <summary>
    /// Where the artist is from, as free text (e.g., "Kinshasa, RDC"). Not a structured
    /// place reference — no geocoding, no lookup table.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxArtistHometownLength)]
    public string? Hometown { get; private set; }

    /// <summary>
    /// ID of the uploaded avatar file tracked in the Storage module. Null until uploaded.
    /// </summary>
    public Guid? AvatarFileId { get; private set; }

    /// <summary>
    /// The identity user UUID of the verified artist account that owns this profile, or
    /// null for a staff-curated, unclaimed profile — the common case at launch, since most
    /// profiles are created by an admin just to group an artist's catalog, with no
    /// associated login. Once set, this is the identity gate the verified-artist fast path
    /// checks — a submission from this exact user id is treated as coming authoritatively
    /// from this artist, never by comparing the submitted artist name as text (names change,
    /// get misspelled, and can collide between unrelated people). No FK to the identity
    /// schema by design, matching every other cross-schema reference in this module.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// When ownership verification completed. Null until <see cref="ClaimOwnership" /> is called.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; private set; }

    /// <summary>
    /// The artist's outbound social platform links, one row per platform. Written only through
    /// <see cref="SetSocialLink" /> and <see cref="RemoveSocialLink" />.
    /// </summary>
    public ICollection<ArtistSocialLinkEntity> SocialLinks { get; } = new List<ArtistSocialLinkEntity>();

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ArtistEntity() { }

    /// <summary>
    /// Creates a new, unclaimed artist profile — typically staff-curated from an existing
    /// lyrics or video record's <c>ArtistName</c>.
    /// </summary>
    /// <param name="id">The unique identifier for the artist profile.</param>
    /// <param name="name">The artist's display name.</param>
    /// <param name="slug">The URL-safe slug for the artist's public page.</param>
    /// <param name="bio">Optional free-text biography.</param>
    /// <param name="realName">The artist's legal or birth name, or null when unknown.</param>
    /// <param name="aliases">Alternate names, or null for none.</param>
    /// <param name="birthdate">The artist's date of birth, or null when unknown.</param>
    /// <param name="hometown">Where the artist is from, or null when unknown.</param>
    /// <param name="today">The current date, against which a future birthdate is rejected.</param>
    /// <returns>A new, unclaimed <see cref="ArtistEntity" />.</returns>
    public static ArtistEntity Create(
        Guid id,
        string name,
        string slug,
        string? bio,
        string? realName,
        IReadOnlyList<string>? aliases,
        DateOnly? birthdate,
        string? hometown,
        DateOnly today
    )
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.ArtistNameRequired);
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.ArtistSlugRequired);
        }

        GuardBirthdate(birthdate: birthdate, today: today);

        var artist = new ArtistEntity
        {
            Id = id,
            Name = name,
            Slug = slug,
            Bio = bio,
            RealName = realName,
            Birthdate = birthdate,
            Hometown = hometown,
        };

        artist.ReplaceAliases(aliases: aliases);
        artist.RecomputeNameIndexes();
        artist.AddDomainEvent(new ArtistChangedEvent(ArtistId: id));

        return artist;
    }
}
