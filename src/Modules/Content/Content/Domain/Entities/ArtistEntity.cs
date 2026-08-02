using System.ComponentModel.DataAnnotations;
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
    /// <param name="errors">The errors factory instance.</param>
    /// <returns>A new, unclaimed <see cref="ArtistEntity" />.</returns>
    public static ArtistEntity Create(Guid id, string name, string slug, string? bio, ArtistErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw errors.NameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw errors.SlugRequired();
        }

        return new ArtistEntity
        {
            Id = id,
            Name = name,
            Slug = slug,
            Bio = bio,
        };
    }

    /// <summary>
    /// Updates the artist's editable profile fields. Slug is immutable after creation to
    /// preserve public URLs — this method never accepts or changes it.
    /// </summary>
    /// <param name="name">The artist's display name.</param>
    /// <param name="bio">Optional free-text biography, or null to clear it.</param>
    /// <param name="errors">The errors factory instance.</param>
    public void Update(string name, string? bio, ArtistErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw errors.NameRequired();
        }

        Name = name;
        Bio = bio;
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
