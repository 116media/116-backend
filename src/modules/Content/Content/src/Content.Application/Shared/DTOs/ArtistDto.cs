namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for an artist profile.
/// </summary>
/// <param name="Id">
/// The unique identifier of the artist profile.
/// </param>
/// <param name="Name">
/// The artist's display name.
/// </param>
/// <param name="Slug">
/// The URL-safe slug used in the artist's public page URL.
/// </param>
/// <param name="Bio">
/// The artist's free-text biography, or null if not yet curated.
/// </param>
/// <param name="AvatarUrl">
/// The publicly accessible URL of the artist's avatar image, resolved from the associated
/// FileEntity. Null if no avatar has been uploaded.
/// </param>
/// <param name="IsVerified">
/// Whether this profile has been claimed and verified by the artist's own account. Derived
/// from the claim state — the claiming user's identity is never exposed.
/// </param>
/// <param name="RealName">
/// The artist's legal or birth name, or null when unknown.
/// </param>
/// <param name="Aliases">
/// Alternate names the artist is known by. Empty when there are none, never null.
/// </param>
/// <param name="Birthdate">
/// The artist's date of birth as a civil date, or null when unknown. Serialised without a
/// timezone so the rendered day is identical for every reader.
/// </param>
/// <param name="Hometown">
/// Where the artist is from as free text, or null when unknown.
/// </param>
/// <param name="SocialLinks">
/// The artist's social platform links, ordered by platform. Empty when there are none,
/// never null.
/// </param>
public record ArtistDto(
    Guid Id,
    string Name,
    string Slug,
    string? Bio,
    string? AvatarUrl,
    bool IsVerified,
    string? RealName,
    IReadOnlyList<string> Aliases,
    DateOnly? Birthdate,
    string? Hometown,
    IReadOnlyList<ArtistSocialLinkDto> SocialLinks
);
