using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a real, addressable artist profile — distinct from the plain-text
/// <c>ArtistName</c> field on <see cref="LyricsEntity" /> and <see cref="VideoEntity" />.
/// A profile can exist unclaimed (staff-curated, no linked account) or claimed by a verified
/// artist account via <see cref="UserId" />.
/// </summary>
public class ArtistEntity : Aggregate<Guid>
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
    public string Slug { get; private set; } = null!;

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
    /// ID of the uploaded avatar file tracked in the Core module. Null until uploaded.
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
    /// <param name="errors">The errors factory instance.</param>
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
        ArtistErrors errors
    )
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw errors.NameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw errors.SlugRequired();
        }

        GuardBirthdate(birthdate: birthdate, errors: errors);

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

        artist.ReplaceAliases(aliases: aliases, errors: errors);
        artist.RecomputeNameIndexes();

        return artist;
    }

    /// <summary>
    /// Updates the artist's editable profile fields. Slug is immutable after creation to
    /// preserve public URLs — this method never accepts or changes it.
    /// </summary>
    /// <param name="name">The artist's display name.</param>
    /// <param name="bio">Optional free-text biography, or null to clear it.</param>
    /// <param name="realName">The artist's legal or birth name, or null to clear it.</param>
    /// <param name="aliases">Alternate names, or null to clear them.</param>
    /// <param name="birthdate">The artist's date of birth, or null to clear it.</param>
    /// <param name="hometown">Where the artist is from, or null to clear it.</param>
    /// <param name="errors">The errors factory instance.</param>
    public void Update(
        string name,
        string? bio,
        string? realName,
        IReadOnlyList<string>? aliases,
        DateOnly? birthdate,
        string? hometown,
        ArtistErrors errors
    )
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw errors.NameRequired();
        }

        GuardBirthdate(birthdate: birthdate, errors: errors);
        ReplaceAliases(aliases: aliases, errors: errors);

        Name = name;
        Bio = bio;
        RealName = realName;
        Birthdate = birthdate;
        Hometown = hometown;

        RecomputeNameIndexes();
    }

    /// <summary>
    /// Normalises and stores the alias list: trimmed, blanks dropped, de-duplicated
    /// case-insensitively, then bounded by count and length. Normalising here rather than in
    /// a validator means the invariant holds for every writer, including seeds.
    /// </summary>
    /// <param name="aliases">The incoming aliases, or null for none.</param>
    /// <param name="errors">The errors factory instance.</param>
    private void ReplaceAliases(IReadOnlyList<string>? aliases, ArtistErrors errors)
    {
        _aliases.Clear();

        if (aliases is null)
        {
            return;
        }

        var seen = new HashSet<string>(comparer: StringComparer.OrdinalIgnoreCase);

        foreach (string alias in aliases)
        {
            string trimmed = alias?.Trim() ?? string.Empty;

            if (trimmed.Length == 0 || !seen.Add(item: trimmed))
            {
                continue;
            }

            if (trimmed.Length > ContentConstants.MaxArtistNameLength)
            {
                throw errors.AliasTooLong();
            }

            _aliases.Add(item: trimmed);
        }

        if (_aliases.Count > ContentConstants.MaxArtistAliasCount)
        {
            throw errors.TooManyAliases();
        }
    }

    /// <summary>
    /// Rejects a birthdate that is not strictly in the past. A future birthdate is bad data,
    /// not a value the profile should render with a negative age.
    /// </summary>
    /// <param name="birthdate">The birthdate to validate, or null.</param>
    /// <param name="errors">The errors factory instance.</param>
    private static void GuardBirthdate(DateOnly? birthdate, ArtistErrors errors)
    {
        if (birthdate is not null && birthdate.Value >= DateOnly.FromDateTime(dateTime: DateTime.UtcNow))
        {
            throw errors.BirthdateInFuture();
        }
    }

    /// <summary>
    /// Recomputes <see cref="NameFolded" /> and <see cref="InitialLetter" /> from the current
    /// <see cref="Name" />.
    /// </summary>
    private void RecomputeNameIndexes()
    {
        NameFolded = FoldName(name: Name);

        InitialLetter =
            NameFolded.Length > 0 && NameFolded[0] is >= 'A' and <= 'Z'
                ? NameFolded[..1]
                : ContentConstants.NonAlphabeticLetterBucket;
    }

    /// <summary>
    /// Strips diacritics, collapses whitespace and upper-cases a display name so that
    /// "Élodie" and "elodie" fold to the same sortable, searchable key.
    /// </summary>
    /// <param name="name">The display name to fold.</param>
    /// <returns>The folded form, safe to sort, bucket and search on.</returns>
    public static string FoldName(string name)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            return string.Empty;
        }

        string collapsed = Regex.Replace(input: name.Trim(), pattern: @"\s+", replacement: " ");
        string decomposed = collapsed.Normalize(normalizationForm: NormalizationForm.FormD);

        var builder = new StringBuilder(capacity: decomposed.Length);

        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch: character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(value: character);
            }
        }

        return builder.ToString().Normalize(normalizationForm: NormalizationForm.FormC).ToUpperInvariant();
    }

    /// <summary>
    /// Sets or clears the avatar file reference.
    /// </summary>
    /// <param name="avatarFileId">The FileEntity ID, or null to clear it.</param>
    public void SetAvatarFileId(Guid? avatarFileId) => AvatarFileId = avatarFileId;

    /// <summary>
    /// Links this profile to a verified artist account. One profile can be claimed by
    /// exactly one account — enforced here and by a database unique constraint on
    /// <c>UserId</c>.
    /// </summary>
    /// <param name="userId">The identity user UUID claiming this profile.</param>
    /// <param name="errors">The errors factory instance.</param>
    /// <exception cref="_116.Shared.Application.Exceptions.ConflictException">
    /// Thrown if the profile is already claimed.
    /// </exception>
    public void ClaimOwnership(Guid userId, ArtistErrors errors)
    {
        if (UserId.HasValue)
        {
            throw errors.AlreadyClaimed();
        }

        UserId = userId;
        VerifiedAt = DateTimeOffset.UtcNow;
    }
}
