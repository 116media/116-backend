using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
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

    /// <summary>
    /// Renames the artist and recomputes the folded-name and initial-letter indexes the
    /// directory browses by. Slug is immutable after creation to preserve public URLs.
    /// </summary>
    /// <param name="name">The artist's display name.</param>
    /// <returns><c>true</c> if the name changed; otherwise <c>false</c>.</returns>
    public bool Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.ArtistNameRequired);
        }

        if (Name == name)
        {
            return false;
        }

        Name = name;

        RecomputeNameIndexes();
        MarkChanged();

        return true;
    }

    /// <summary>
    /// Revises the biographical fields. The alias list is normalised on the way in, so an
    /// incoming list that normalises to the current one writes nothing.
    /// </summary>
    /// <param name="bio">Optional free-text biography, or null to clear it.</param>
    /// <param name="realName">The artist's legal or birth name, or null to clear it.</param>
    /// <param name="aliases">Alternate names, or null to clear them.</param>
    /// <param name="birthdate">The artist's date of birth, or null to clear it.</param>
    /// <param name="hometown">Where the artist is from, or null to clear it.</param>
    /// <param name="today">The current date, against which a future birthdate is rejected.</param>
    /// <returns><c>true</c> if any value changed; otherwise <c>false</c>.</returns>
    public bool ReviseProfile(
        string? bio,
        string? realName,
        IReadOnlyList<string>? aliases,
        DateOnly? birthdate,
        string? hometown,
        DateOnly today
    )
    {
        GuardBirthdate(birthdate: birthdate, today: today);

        List<string> currentAliases = [.. _aliases];
        ReplaceAliases(aliases: aliases);

        bool aliasesChanged = !_aliases.SequenceEqual(currentAliases, StringComparer.Ordinal);

        if (!aliasesChanged && Bio == bio && RealName == realName && Birthdate == birthdate && Hometown == hometown)
        {
            return false;
        }

        Bio = bio;
        RealName = realName;
        Birthdate = birthdate;
        Hometown = hometown;

        MarkChanged();

        return true;
    }

    /// <summary>
    /// Records one change notice per unit of work, however many edit verbs the handler calls.
    /// </summary>
    private void MarkChanged()
    {
        if (DomainEvents.OfType<ArtistChangedEvent>().Any())
        {
            return;
        }

        AddDomainEvent(new ArtistChangedEvent(ArtistId: Id));
    }

    /// <summary>
    /// Normalises and stores the alias list: trimmed, blanks dropped, de-duplicated
    /// case-insensitively, then bounded by count and length. Normalising here rather than in
    /// a validator means the invariant holds for every writer, including seeds.
    /// </summary>
    /// <param name="aliases">The incoming aliases, or null for none.</param>
    private void ReplaceAliases(IReadOnlyList<string>? aliases)
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
                throw new ContentRuleException(ContentRuleCodes.ArtistAliasTooLong);
            }

            _aliases.Add(item: trimmed);
        }

        if (_aliases.Count > ContentConstants.MaxArtistAliasCount)
        {
            throw new ContentRuleException(ContentRuleCodes.ArtistTooManyAliases);
        }
    }

    /// <summary>
    /// Rejects a birthdate that is not strictly in the past. A future birthdate is bad data,
    /// not a value the profile should render with a negative age.
    /// </summary>
    /// <param name="birthdate">The birthdate to validate, or null.</param>
    private static void GuardBirthdate(DateOnly? birthdate, DateOnly today)
    {
        if (birthdate is not null && birthdate.Value >= today)
        {
            throw new ContentRuleException(ContentRuleCodes.ArtistBirthdateInFuture);
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
    public void SetAvatarFileId(Guid? avatarFileId)
    {
        AvatarFileId = avatarFileId;
        AddDomainEvent(new ArtistChangedEvent(ArtistId: Id));
    }

    /// <summary>
    /// Links this profile to a verified artist account. One profile can be claimed by
    /// exactly one account — enforced here and by a database unique constraint on
    /// <c>UserId</c>.
    /// </summary>
    /// <param name="userId">The identity user UUID claiming this profile.</param>
    /// <exception cref="ContentRuleException">
    /// Thrown if the profile is already claimed.
    /// </exception>
    public void ClaimOwnership(Guid userId, DateTimeOffset now)
    {
        if (UserId.HasValue)
        {
            throw new ContentRuleException(ContentRuleCodes.ArtistAlreadyClaimed);
        }

        UserId = userId;
        VerifiedAt = now;

        AddDomainEvent(new ArtistOwnershipVerifiedEvent(ArtistId: Id, UserId: userId));
    }

    /// <summary>
    /// Adds or replaces the link for a platform, reporting false when the stored URL already
    /// matches so an unchanged upsert writes nothing.
    /// </summary>
    /// <param name="platform">The social platform the link points to.</param>
    /// <param name="url">The outbound profile URL.</param>
    /// <returns><c>true</c> if a link was added or its URL changed; otherwise <c>false</c>.</returns>
    public bool SetSocialLink(EnumSocialPlatform platform, string url)
    {
        ArtistSocialLinkEntity? existing = FindSocialLink(platform: platform);

        if (existing is null)
        {
            SocialLinks.Add(
                ArtistSocialLinkEntity.Create(id: Guid.NewGuid(), artistId: Id, platform: platform, url: url)
            );

            return true;
        }

        if (existing.Url == url)
        {
            return false;
        }

        existing.UpdateUrl(url: url);

        return true;
    }

    /// <summary>
    /// Removes the link for a platform, reporting whether one was there.
    /// </summary>
    /// <param name="platform">The social platform whose link is removed.</param>
    /// <returns><c>true</c> if a link was removed; otherwise <c>false</c>.</returns>
    public bool RemoveSocialLink(EnumSocialPlatform platform)
    {
        ArtistSocialLinkEntity? existing = FindSocialLink(platform: platform);

        if (existing is null)
        {
            return false;
        }

        SocialLinks.Remove(existing);

        return true;
    }

    /// <summary>
    /// Returns this artist's link for a platform, or null when the slot is empty.
    /// </summary>
    /// <param name="platform">The social platform to look up.</param>
    /// <returns>The matching link, or <c>null</c>.</returns>
    public ArtistSocialLinkEntity? FindSocialLink(EnumSocialPlatform platform)
    {
        return SocialLinks.FirstOrDefault(link => link.Platform == platform);
    }
}
